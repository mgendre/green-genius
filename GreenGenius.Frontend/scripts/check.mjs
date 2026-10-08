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

const translations = JSON.parse(readFileSync(join(frontend, 'public/i18n/fr.json'), 'utf8'));
const keysOf = (expression) => expression.match(/'[a-z0-9-]+(\.[a-z0-9-]+)+'/g) ?? [];

const arrayOf = (component) =>
  `Hlm${component
    .split('-')
    .map((part) => part[0].toUpperCase() + part.slice(1))
    .join('')}Imports`;

const rules = [
  {
    extensions: ['.html'],
    pattern: /\s([a-zA-Z][\w.-]*)="([^"{]*\|\s*translate\b[^"]*)"/g,
    message: ([, name, value]) =>
      `the translate pipe needs a binding: [${name.startsWith('aria-') ? 'attr.' : ''}${name}]="${value}"`,
  },
  {
    extensions: ['.html'],
    pattern: /((?:'[^']*'|[^|{}"'])*)\|\s*translate\b/g,
    applies: ([, expression]) =>
      keysOf(expression).some((key) => !(key.slice(1, -1) in translations)),
    message: ([, expression]) =>
      `${keysOf(expression)
        .filter((key) => !(key.slice(1, -1) in translations))
        .join(', ')} is not in public/i18n/fr.json: use an existing key or add it there`,
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
];

const lineOf = (content, match) => {
  const start = match.index + match[0].length - match[0].trimStart().length;
  return content.slice(0, start).split('\n').length;
};

const conventions = filesOf(app, ['.html', '.ts'])
  .filter((file) => !notOurs.some((directory) => file.startsWith(directory)))
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
  });

const build = spawnSync(join(frontend, 'node_modules/.bin/ng'), ['build'], {
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
console.log(
  failed
    ? `check: FAILED, build: ${buildFailed ? Math.max(buildProblems, 1) : 0} to fix, conventions: ${conventions.length} to fix`
    : 'check: OK',
);
process.exit(failed ? 1 : 0);
