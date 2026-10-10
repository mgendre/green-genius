import { readFileSync, writeFileSync } from 'node:fs';

const path = new URL('./public/i18n/fr.json', import.meta.url);
const source = readFileSync(path, 'utf8');
const translations = JSON.parse(source);
const keys = Object.keys(translations);
const lineCount = source.split('\n').filter((line) => line.trimStart().startsWith('"')).length;

if (lineCount !== keys.length) {
  console.error(`sortkeys: FAILED, ${lineCount - keys.length} duplicate key(s) in fr.json, nothing written`);
  process.exit(1);
}

const sorted = Object.fromEntries(keys.sort().map((key) => [key, translations[key]]));
const output = `${JSON.stringify(sorted, null, 2)}\n`;

if (output === source) {
  console.log(`sortkeys: OK, already sorted (${keys.length} keys)`);
} else {
  writeFileSync(path, output);
  console.log(`sortkeys: OK, fr.json sorted (${keys.length} keys)`);
}
