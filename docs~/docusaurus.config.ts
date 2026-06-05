import { themes as prismThemes } from 'prism-react-renderer';
import type { Config } from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const repositoryUrl = 'https://github.com/achieveonepark/lite-db';

const config: Config = {
  title: 'LiteDB',
  tagline: 'LiteDB documentation',
  favicon: 'favicon.svg',

  // GitHub Pages — url/baseUrl 은 통합 빌드(CI)가 덮어쓴다.
  url: 'https://achieveonepark.github.io',
  baseUrl: '/lite-db/',
  organizationName: 'achieveonepark',
  projectName: 'lite-db',

  onBrokenLinks: 'warn',
  onBrokenAnchors: 'warn',

  markdown: {
    mermaid: true,
    format: 'detect',
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },
  themes: ['@docusaurus/theme-mermaid'],

  clientModules: ['./src/clientModules/themeReset.js'],

  i18n: {
    defaultLocale: 'ko',
    locales: ['ko', 'en', 'ja', 'zh'],
    localeConfigs: {
      ko: { label: '한국어', htmlLang: 'ko-KR' },
      en: { label: 'English', htmlLang: 'en-US' },
      ja: { label: '日本語', htmlLang: 'ja-JP' },
      zh: { label: '中文', htmlLang: 'zh-CN' },
    },
  },

  presets: [
    [
      'classic',
      {
        docs: {
          path: 'docs',
          routeBasePath: '/',
          sidebarPath: './sidebars.ts',
          showLastUpdateTime: true,
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    image: 'logo.svg',
    colorMode: {
      defaultMode: 'dark',
      respectPrefersColorScheme: false,
      disableSwitch: false,
    },
    navbar: {
      title: 'LiteDB',
      logo: {
        alt: 'LiteDB',
        src: 'logo.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docs',
          position: 'left',
          label: 'Docs',
        },
        {
          type: 'localeDropdown',
          position: 'right',
        },
        {
          href: repositoryUrl,
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [],
      copyright: 'MIT License · Copyright © LiteDB',
    },
    prism: {
      theme: prismThemes.oneDark,
      darkTheme: prismThemes.oneDark,
      additionalLanguages: ['csharp', 'json', 'yaml', 'bash'],
    },
    mermaid: {
      theme: { light: 'base', dark: 'base' },
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
