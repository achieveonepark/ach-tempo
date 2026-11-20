import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { defineConfig } from '@rspress/core';

const docsDirectory = path.dirname(fileURLToPath(import.meta.url));

export default defineConfig({
  root: path.join(docsDirectory, '.rspress-docs'),
  base: '/ach-tempo/',
  outDir: 'out',
  lang: 'ko',
  title: 'Tempo',
  description:
    '디렉터가 조건만 깔면 월드가 받아 상황을 키우는, 긴장 강약 자동 조절 시스템.',
  icon: '/icon.svg',
  locales: [
    {
      lang: 'ko',
      label: '한국어',
      title: 'Tempo',
      description:
        '디렉터가 조건만 깔면 월드가 받아 상황을 키우는, 긴장 강약 자동 조절 시스템.',
    },
    {
      lang: 'en',
      label: 'English',
      title: 'Tempo',
      description:
        'An automatic tension-pacing system where the director seeds conditions and the world grows the situation.',
    },
    {
      lang: 'ja',
      label: '日本語',
      title: 'Tempo',
      description:
        'ディレクターが条件を撒くだけで、ワールドが状況を育てる緊張度自動調整システム。',
    },
    {
      lang: 'zh',
      label: '简体中文',
      title: 'Tempo',
      description:
        '导演铺设条件后由世界承接并放大局势的紧张度自动调节系统。',
    },
  ],
  route: {
    localeRedirect: 'never',
    cleanUrls: true,
  },
  globalStyles: path.join(docsDirectory, 'styles.css'),
  themeConfig: {
    darkMode: 'dark',
    socialLinks: [
      {
        icon: 'github',
        mode: 'link',
        content: 'https://github.com/achieveonepark/ach-tempo',
      },
    ],
  },
});
