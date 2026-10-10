import { spawnSync } from 'node:child_process';
import { readdirSync, readFileSync } from 'node:fs';
import { join, relative } from 'node:path';

const frontend = join(import.meta.dirname, '..');
const repository = join(frontend, '..');
const app = join(frontend, 'src/app');
const helm = join(app, 'shared/spartan/helm');
const notOurs = [helm, join(app, 'api')];
const maxBuildLines = 40;

const filesOf = (directory, extensions) =>
  readdirSync(directory, { recursive: true, withFileTypes: true })
    .filter(
      (entry) => entry.isFile() && extensions.some((extension) => entry.name.endsWith(extension)),
    )
    .map((entry) => join(entry.parentPath, entry.name));

const helmNames = new Set(
  filesOf(helm, ['.ts']).flatMap(
    (file) => readFileSync(file, 'utf8').match(/hlm[A-Za-z0-9-]*/g) ?? [],
  ),
);

const translationsFile = join(frontend, 'public/i18n/fr.json');
const translations = JSON.parse(readFileSync(translationsFile, 'utf8'));
const translationLines = readFileSync(translationsFile, 'utf8').split('\n');
const keysOf = (expression) => expression.match(/'[a-z0-9-]+(\.[a-z0-9-]+)+'/g) ?? [];

const expressionBefore = ({ input, index }) => {
  let start = index;
  while (start > 0) {
    const character = input[start - 1];
    if (character === "'") {
      const opening = input.lastIndexOf("'", start - 2);
      if (opening < 0) {
        break;
      }
      start = opening;
    } else if ('|{}"'.includes(character)) {
      break;
    } else {
      start -= 1;
    }
  }
  return input.slice(start, index);
};

const missingKeysOf = (match) =>
  keysOf(expressionBefore(match)).filter((key) => !(key.slice(1, -1) in translations));

const arrayOf = (component) =>
  `Hlm${component
    .split('-')
    .map((part) => part[0].toUpperCase() + part.slice(1))
    .join('')}Imports`;

const keyLiteralPattern = /(['"])(([a-z0-9-]+)(?:\.[a-z0-9-]+)+)\1/g;
const stringLiteralPattern = /'(?:[^'\\\n]|\\.)*'|"(?:[^"\\\n]|\\.)*"|`(?:[^`\\]|\\.)*`/g;
const isFrenchText = (text) => /[àâäçéèêëîïôöûùüÿœ]/.test(text) || text === 'Oui' || text === 'Non';

const rules = [
  {
    extensions: ['.html'],
    pattern: /\s([a-zA-Z][\w.-]*)="([^"{]*\|\s*translate\b[^"]*)"/g,
    message: ([, name, value]) =>
      `the translate pipe needs a binding: [${name.startsWith('aria-') ? 'attr.' : ''}${name}]="${value}"`,
  },
  {
    extensions: ['.html'],
    pattern: /\|\s*translate\b/g,
    applies: (match) => missingKeysOf(match).length > 0,
    message: (match) =>
      `${missingKeysOf(match).join(', ')} is not in public/i18n/fr.json: use an existing key or add it there`,
  },
  {
    extensions: ['.html'],
    pattern: /(?<!\|)\s\[?translate\]?(?=[=\s>/])/g,
    message: () => `no translate directive: use the pipe, {{ 'key' | translate }}`,
  },
  {
    extensions: ['.html'],
    pattern: /(?<![A-Za-z0-9-])hlm[A-Za-z0-9-]*/g,
    applies: ([name]) => !helmNames.has(name),
    message: ([name]) =>
      `'${name}' is not a Spartan Helm name: copy it from the skill or from src/app/shared/spartan/helm`,
  },
  {
    extensions: ['.html', '.ts'],
    pattern: /\b[Hh]l[a-ln-z](?=[A-Z-])[\w-]*/g,
    message: ([name]) => `'${name}' is misspelt: Spartan names start with Hlm or hlm`,
  },
  {
    extensions: ['.ts'],
    pattern: /import\s*\{([^}]*)\}\s*from\s*'@spartan-ng\/helm\/([^']+)'/g,
    applies: ([, names, component]) =>
      component !== 'utils' && names.split(',').filter((name) => name.trim()).length > 1,
    message: ([, , component]) =>
      `import the array, one name: import { ${arrayOf(component)} } from '@spartan-ng/helm/${component}'`,
  },
  {
    extensions: ['.ts'],
    pattern: /\b(BrnDialogRef|HlmDialogService)\b/g,
    message: ([name]) => `no ${name}: a dialog is opened by a signal, see the spartan-dialog skill`,
  },
  {
    extensions: ['.ts'],
    pattern: keyLiteralPattern,
    applies: ([, , key]) => !Object.hasOwn(translations, key),
    message: ([, , key]) =>
      `'${key}' is not in public/i18n/fr.json: use an existing key or add it there`,
  },
  {
    extensions: ['.ts'],
    pattern: stringLiteralPattern,
    applies: ([literal]) => isFrenchText(literal.slice(1, -1)),
    message: () => 'French text in code: move it to public/i18n/fr.json and use the key',
  },
  {
    extensions: ['.html'],
    pattern: /(?<![-:\w])\w+!(?!=)|[)\]]!(?!=)/g,
    message: () => 'no ! in a template: test the value with @if and use its alias',
  },
];

const lineOf = (content, match) => {
  const start = match.index + match[0].length - match[0].trimStart().length;
  return content.slice(0, start).split('\n').length;
};

const isOurs = (file) => !notOurs.some((directory) => file.startsWith(directory));
const ourTsFiles = filesOf(app, ['.ts']).filter(isOurs);
const sourceContents = filesOf(join(frontend, 'src'), ['.ts']).map((file) => ({
  file,
  content: readFileSync(file, 'utf8'),
}));
const inputPattern = /\binput(\.required)?(<[^(]*>)?\(/g;
const problem = (file, content, match, message) =>
  `${relative(repository, file)}:${lineOf(content, match)}  ${message}`;

const orphanComponentsOf = (file) => {
  const content = readFileSync(file, 'utf8');
  if (!content.includes('@Component(')) {
    return [];
  }
  return [...content.matchAll(/export class (\w+)/g)]
    .filter(
      ([, name]) =>
        !sourceContents.some(
          (source) => source.file !== file && new RegExp(`\\b${name}\\b`).test(source.content),
        ),
    )
    .map((match) =>
      problem(
        file,
        content,
        match,
        `${match[1]} is used by no route and no component: wire it or delete it`,
      ),
    );
};

const routedComponentNames = filesOf(app, ['.routes.ts']).flatMap((file) =>
  [...readFileSync(file, 'utf8').matchAll(/\bcomponent:\s*(\w+)/g)].map(([, name]) => name),
);

const unboundInputsOf = (file) => {
  const content = readFileSync(file, 'utf8');
  const isRouted = [...content.matchAll(/export class (\w+)/g)].some(([, name]) =>
    routedComponentNames.includes(name),
  );
  if (!isRouted) {
    return [];
  }
  return [...content.matchAll(inputPattern)].map((match) =>
    problem(
      file,
      content,
      match,
      'route inputs are not bound: add withComponentInputBinding() to provideRouter in app.config.ts',
    ),
  );
};

const inputBindingEnabled = readFileSync(join(app, 'app.config.ts'), 'utf8').includes(
  'withComponentInputBinding',
);

const wiringProblems = [
  ...ourTsFiles.flatMap(orphanComponentsOf),
  ...(inputBindingEnabled ? [] : ourTsFiles.flatMap(unboundInputsOf)),
];

const ourFiles = filesOf(app, ['.html', '.ts']).filter(isOurs);
const translationsPath = relative(repository, translationsFile);
const translationKeys = Object.keys(translations);
const translationLineOf = (key) =>
  translationLines.findIndex((line) => line.trimStart().startsWith(`${JSON.stringify(key)}:`)) + 1;

const unsortedKeyProblems = translationKeys
  .map((key, index) => ({ key, previous: translationKeys[index - 1] }))
  .filter(({ key, previous }) => previous !== undefined && key < previous)
  .map(
    ({ key, previous }) =>
      `${translationsPath}:${translationLineOf(key)}  '${key}' must come before '${previous}': keep fr.json sorted`,
  );

const usedKeys = new Set(
  ourFiles.flatMap((file) =>
    [...readFileSync(file, 'utf8').matchAll(keyLiteralPattern)].map(([, , key]) => key),
  ),
);

const unusedKeyProblems = translationKeys
  .filter((key) => !usedKeys.has(key))
  .map(
    (key) => `${translationsPath}:${translationLineOf(key)}  '${key}' is used nowhere: delete it`,
  );

const duplicateKeyProblemsOf = (lines) => {
  const seen = new Set();
  return lines.flatMap((line, index) => {
    const key = line.match(/^ {2}"([^"]+)":/)?.[1];
    if (key === undefined) {
      return [];
    }
    if (!seen.has(key)) {
      seen.add(key);
      return [];
    }
    return [`${translationsPath}:${index + 1}  '${key}' is written twice: delete the duplicate`];
  });
};

const duplicateKeyProblems = duplicateKeyProblemsOf(translationLines);

const conventions = ourFiles
  .flatMap((file) => {
    const content = readFileSync(file, 'utf8');
    return rules
      .filter((rule) => rule.extensions.some((extension) => file.endsWith(extension)))
      .flatMap((rule) =>
        [...content.matchAll(rule.pattern)]
          .filter((match) => rule.applies?.(match) ?? true)
          .map(
            (match) =>
              `${relative(repository, file)}:${lineOf(content, match)}  ${rule.message(match)}`,
          ),
      );
  })
  .concat(wiringProblems, unsortedKeyProblems, unusedKeyProblems, duplicateKeyProblems)
  .filter((line, index, lines) => lines.indexOf(line) === index);

const build =spawnSync(join(frontend, 'node_modules/.bin/ng'), ['build'], {
  cwd: frontend,
  encoding: 'utf8',
  env: { ...process.env, NO_COLOR: '1' },
});
const buildLines = `${build.stdout ?? ''}${build.stderr ?? ''}${build.error?.message ?? ''}`.split(
  '\n',
);
const buildProblems = buildLines.filter((line) => /\[(ERROR|WARNING)\]/.test(line)).length;
const buildFailed = build.status !== 0 || buildProblems > 0;

if (buildFailed) {
  const start = Math.max(
    0,
    buildLines.findIndex((line) => /\[(ERROR|WARNING)\]/.test(line)),
  );
  console.log(
    buildLines
      .slice(start, start + maxBuildLines)
      .join('\n')
      .trim(),
  );
}
conventions.forEach((line) => console.log(line));

const failed = buildFailed || conventions.length > 0;
const buildToFix = buildFailed ? Math.max(buildProblems, 1) : 0;
console.log(
  failed
    ? `check: FAILED, build: ${buildToFix} to fix, conventions: ${conventions.length} to fix`
    : 'check: OK',
);
process.exit(failed ? 1 : 0);
