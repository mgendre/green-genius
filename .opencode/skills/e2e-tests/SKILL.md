---
name: e2e-tests
description: "How to write a Playwright test of a frontend screen here — one spec per page in GreenGenius.Frontend/e2e/, the API mocked with mockApi, French texts and roles as locators. Read when a task adds or changes a page, a list, a form or a dialog a person uses."
---

# Playwright tests of a screen

The browser drives the real Angular app served by `ng serve`; the API is never called: every `/api` request is answered
by `mockApi` from `GreenGenius.Frontend/e2e/api-mock.ts`. Every spec imports `test` and `expect` from `./fixtures`,
never from `@playwright/test`: the fixture fails a test when the browser logs an error or throws. Reference:
`GreenGenius.Frontend/e2e/planner.spec.ts`. Config: `GreenGenius.Frontend/playwright.config.ts`, never changed by a
task; Chromium is the only browser, never add another project. CI runs `npm run e2e` in the `frontend` job of
`.github/workflows/ci.yaml`.

| The task                           | The test                                                                   |
|------------------------------------|----------------------------------------------------------------------------|
| a new page                         | a new `e2e/<feature>.spec.ts`                                              |
| a change on a page that has a spec | new `test(...)` blocks in that spec, the old ones kept green               |
| a list or a view of data           | one test for the empty state, one with data                                |
| a form or a dialog                 | one test that fills and submits it, and asserts what the person sees after |
| a pure style change                | no test                                                                    |

## Steps

1. Read `e2e/api-mock.ts` and `e2e/planner.spec.ts`.
2. Build the data with `aGarden()` and `aBed(garden)` from `e2e/builders.ts` (`aBed(garden, { name: 'Planche Nord' })`
   overrides a field): never a literal id and never a DTO written out twice. Add a builder there when a third DTO is
   needed, with `import type` from the generated client. A mock set-up that two tests of a spec share is one function of
   that spec, or a `test.beforeEach`; each test still gets its own page and its own data.
3. In each test, `await mockApi(page, { '<METHOD> <path>': <body> })` before `page.goto`. The key is the method, a
   space, the path without `/api` and without the query: `'GET /gardens'`, `'POST /gardens'`,
   `'GET /gardens/<id>/beds'`. The body follows the DTO in `src/app/api/api-client.generated.ts`. A request with no key
   gets a 404: mock every request the page makes.
4. `await page.goto('/<route>')`.
5. Act and assert with locators by role and French text, copied from `public/i18n/fr.json`:
   `page.getByRole('button', { name: 'Ajouter' })`, `page.getByText('Aucun jardin pour le moment')`,
   `page.getByLabel('Nom')`. Assert with `await expect(locator).toBeVisible()`, `toHaveText`, `toHaveValue`,
   `toHaveCount(0)` or `toBeHidden()`. A text that is a part of another one ("Tomate" in "Planche Tomates") takes
   `{ exact: true }`. A button name that appears on several rows is narrowed by chaining:
   `page.getByRole('listitem').filter({ hasText: 'Potager' }).getByRole('button', { name: 'Supprimer' })`; a dialog is
   `page.getByRole('dialog')` or `getByRole('alertdialog')`.
6. Run `npm run e2e --prefix GreenGenius.Frontend`. It ends with `<n> passed`; a failure prints the test, the locator
   and the line.
7. A failure `Browser errors: fix the app code that raises them` lists the console errors (as
   `NG0201: No provider found for ...`): fix the app code under `src/app/` that raises them, with the skill of that
   component (a dialog: `spartan-dialog`), never the test or the fixture. An HTTP status the test mocked itself (a 500)
   is not reported.

`npm run check` lints `e2e/` with `eslint-plugin-playwright`: a line it prints (`playwright/no-wait-for-timeout`,
`prefer-web-first-assertions`, `no-nth-methods`, `no-focused-test`) is fixed in the test, never silenced.

Every spec has one `test.describe('on a phone')` with `test.use({ viewport: PHONE_VIEWPORT })` that opens the page with
its widest content (a list with data, an open sheet) and calls `expectNoHorizontalScroll(page)`; both come from
`e2e/layout.ts`. A failure is fixed in the styles or the template of the page, with the skills `scss-styling` and
`ui-quality`.

Braces `{ }` on every `if`, `else`, `for` and `while`, also around a single line.

## Reference

```ts
import { expect, test } from './fixtures';
import { mockApi } from './api-mock';
import { aGarden } from './builders';

test('lists the gardens returned by the API', async ({ page }) => {
  const garden = aGarden({ name: 'Potager' });
  await mockApi(page, {
    'GET /gardens': [garden],
    [`GET /gardens/${garden.id}/beds`]: [],
  });

  await page.goto('/planner');

  await expect(page.getByRole('button', { name: 'Potager' })).toBeVisible();
});
```

## Never

- An accessibility test (axe, ARIA snapshot): accessibility is out of scope for now, decided by the user.
- `page.waitForTimeout`, a CSS class, a tag selector or an `nth()` as a locator: use a role, a label or a text.
- `expect(await locator.isVisible()).toBe(true)` and the like: the check does not wait. Write
  `await expect(locator).toBeVisible()`.
- `test.only`, `test.skip`, a retry, a change of `playwright.config.ts`, `api-mock.ts` or `fixtures.ts` to make a test
  pass.
- A test that calls the real API or needs the database.
- Change the app code to make a test pass when the test is wrong: fix the test; when the app is wrong, fix the app.
