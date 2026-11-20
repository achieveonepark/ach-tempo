import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const directory = path.dirname(fileURLToPath(import.meta.url));
const sourceRoot = path.join(directory, 'content/docs');
const outputRoot = path.join(directory, '.rspress-docs');
const outputDirectory = path.join(directory, 'out');
const languages = ['ko', 'en', 'ja', 'zh'];

function metadataFor(sourceDirectory, language) {
  const localizedPath = path.join(sourceDirectory, `meta.${language}.json`);
  const defaultPath = path.join(sourceDirectory, 'meta.json');
  const metadataPath = fs.existsSync(localizedPath) ? localizedPath : defaultPath;
  return fs.existsSync(metadataPath)
    ? JSON.parse(fs.readFileSync(metadataPath, 'utf8'))
    : undefined;
}

function makeMetaItems(sourceDirectory, language, pages) {
  return pages.map((page) => {
    const childDirectory = path.join(sourceDirectory, page);
    const childMetadata = fs.existsSync(childDirectory)
      ? metadataFor(childDirectory, language)
      : undefined;

    return childMetadata
      ? {
          type: 'dir',
          name: page,
          label: childMetadata.title,
        }
      : page;
  });
}

function copyLocale(sourceDirectory, relativeDirectory, language) {
  const entries = fs.readdirSync(sourceDirectory, { withFileTypes: true });

  for (const entry of entries) {
    const sourcePath = path.join(sourceDirectory, entry.name);
    const relativePath = path.join(relativeDirectory, entry.name);

    if (entry.isDirectory()) {
      copyLocale(sourcePath, relativePath, language);
      continue;
    }

    if (entry.name === 'meta.json' || /^meta\.(en|ja|zh)\.json$/.test(entry.name)) {
      continue;
    }

    if (entry.name.endsWith('.mdx')) {
      const extension = path.extname(entry.name);
      const basename = path.basename(entry.name, extension);
      const localizedMatch = basename.match(/\.(en|ja|zh)$/);
      const fileLanguage = localizedMatch?.[1] ?? 'ko';
      if (fileLanguage !== language) continue;

      const outputName = localizedMatch
        ? `${basename.slice(0, -localizedMatch[0].length)}${extension}`
        : entry.name;
      const outputPath = path.join(outputRoot, language, relativeDirectory, outputName);
      fs.mkdirSync(path.dirname(outputPath), { recursive: true });
      fs.copyFileSync(sourcePath, outputPath);
      continue;
    }

    const outputPath = path.join(outputRoot, language, relativePath);
    fs.mkdirSync(path.dirname(outputPath), { recursive: true });
    fs.copyFileSync(sourcePath, outputPath);
  }

  const metadata = metadataFor(sourceDirectory, language);
  if (metadata?.pages) {
    const outputPath = path.join(outputRoot, language, relativeDirectory, '_meta.json');
    fs.mkdirSync(path.dirname(outputPath), { recursive: true });
    fs.writeFileSync(
      outputPath,
      `${JSON.stringify(makeMetaItems(sourceDirectory, language, metadata.pages), null, 2)}\n`,
    );
  }
}

function createLocaleAliases() {
  const koreanFiles = [];
  function collect(sourceDirectory, relativeDirectory = '') {
    for (const entry of fs.readdirSync(sourceDirectory, { withFileTypes: true })) {
      const relativePath = path.join(relativeDirectory, entry.name);
      const absolutePath = path.join(sourceDirectory, entry.name);
      if (entry.isDirectory()) {
        collect(absolutePath, relativePath);
      } else if (entry.name.endsWith('.mdx') && !/\.(en|ja|zh)\.mdx$/.test(entry.name)) {
        koreanFiles.push(relativePath.slice(0, -'.mdx'.length));
      }
    }
  }
  collect(sourceRoot);

  for (const route of koreanFiles) {
    const target = route === 'index' ? '/ach-tempo/' : `/ach-tempo/${route}`;
    const aliasDirectory = path.join(outputDirectory, 'ko', route === 'index' ? '' : route);
    const aliasPath = path.join(aliasDirectory, 'index.html');
    fs.mkdirSync(aliasDirectory, { recursive: true });
    fs.writeFileSync(
      aliasPath,
      `<!doctype html><html lang="ko"><head><meta charset="utf-8"><script>location.replace(${JSON.stringify(target)} + location.search + location.hash)</script><noscript><meta http-equiv="refresh" content="0;url=${target}"></noscript><link rel="canonical" href="${target}"><title>Tempo</title></head><body><a href="${target}">계속하기</a></body></html>\n`,
    );
  }
}

if (process.argv[2] === '--aliases') {
  createLocaleAliases();
} else {
  fs.rmSync(outputRoot, { recursive: true, force: true });

  for (const language of languages) {
    copyLocale(sourceRoot, '', language);
  }

  const publicDirectory = path.join(directory, 'public');
  fs.cpSync(publicDirectory, path.join(outputRoot, 'public'), { recursive: true });
}
