---
name: screenshots
description: "How to check a screen by looking at its screenshots — npm run screens, the PNG files, what to compare them with, what to report. Read after check and e2e pass, on a task that changes what a person sees and names this skill."
---

# Looking at the screenshots

`npm run screens --prefix GreenGenius.Frontend` opens each page with mocked data and writes two PNG per state in
`GreenGenius.Frontend/screenshots/`: `<name>-desktop.png` (1280 px) and `<name>-phone.png` (390 px). Names today:
`planner-main`, `planner-garden-dialog`, `encyclopedia-empty`, `encyclopedia-sheet`. Specs:
`GreenGenius.Frontend/e2e/screens/*.shots.ts`; model: `planner.shots.ts`.

## Steps

One visual pass per plan, at the end: only the last task of a plan that names this skill looks at the PNG; the earlier
tasks stop at `check` and `e2e`, with geometry asserted by bounding boxes. A task given alone (not in a plan) does the
pass itself.

1. After `check: OK` and a green `npm run e2e --prefix GreenGenius.Frontend`, run
   `npm run screens --prefix GreenGenius.Frontend 2>&1 | tail -20`. It ends with `passed`.
2. With the read tool, read the PNG of each state the task changed, one image per call, the desktop one only; read the
   phone one too when a criterion is about the phone: `GreenGenius.Frontend/screenshots/<name>-<width>.png`.
3. For each criterion of the task, say what you see. Then look for these defects: text cut or ending in `…`; a text
   under another text; a button or an icon on top of a label; content past the right edge or outside its card; a column
   too narrow to read a word; a blank strip or a large empty area inside a card; left edges that do not line up; spacing
   that differs between similar blocks.
4. Fix what you saw in the template or the styles, with the skills of the task. Run
   `npm run check --prefix GreenGenius.Frontend`, `npm run screens --prefix GreenGenius.Frontend` and read the PNG
   again. At most 2 rounds.
5. A new page or dialog gets its own `e2e/screens/<page>.shots.ts`: copy `planner.shots.ts` (`mockApi`, builders, one
   `expect` that the state is shown, then `captureAtAllWidths(page, '<name>')`).

## Rules

- Describe only what you see. Quote texts as they appear on the image; when unsure of a text, look it up in
  `public/i18n/fr.json`. Never invent a text, a grid or a column that is not on the image.
- Never guess a cause from the image: the cause is in the code.
- The image shows neither a horizontal scroll nor a hidden element: `npm run e2e` tests the scroll. A thing you cannot
  judge from the image is `not verified`.
- Alignment, equal heights and equal widths are not judged by eye: assert them with bounding boxes in the e2e spec
  (`boundingBox()`), then the image only confirms. Observed: cards reported even stayed ragged, a title block kept an
  orphan background, one title centred while another was left-aligned.
- Compare each page with its sibling page: same title alignment, same rule width, same heading style. A difference
  between pages is a defect.
- A background behind a title or a text that stops short of its container is a defect.
- Accessibility is out of scope (decided by the user): no remark on it.
- Never commit a PNG, never change `capture.ts` or `playwright.screens.config.ts`.

## Report

One line per image read, in the report of the task:

```
Screens: planner-main-phone — the garden list stacks above the beds, no text cut; fixed: the bed title wrapped under its buttons
```
