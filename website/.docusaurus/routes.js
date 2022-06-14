
import React from 'react';
import ComponentCreator from '@docusaurus/ComponentCreator';

export default [
  {
    path: '/therb-gh/__docusaurus/debug',
    component: ComponentCreator('/therb-gh/__docusaurus/debug','576'),
    exact: true
  },
  {
    path: '/therb-gh/__docusaurus/debug/config',
    component: ComponentCreator('/therb-gh/__docusaurus/debug/config','e39'),
    exact: true
  },
  {
    path: '/therb-gh/__docusaurus/debug/content',
    component: ComponentCreator('/therb-gh/__docusaurus/debug/content','c41'),
    exact: true
  },
  {
    path: '/therb-gh/__docusaurus/debug/globalData',
    component: ComponentCreator('/therb-gh/__docusaurus/debug/globalData','da6'),
    exact: true
  },
  {
    path: '/therb-gh/__docusaurus/debug/metadata',
    component: ComponentCreator('/therb-gh/__docusaurus/debug/metadata','456'),
    exact: true
  },
  {
    path: '/therb-gh/__docusaurus/debug/registry',
    component: ComponentCreator('/therb-gh/__docusaurus/debug/registry','18e'),
    exact: true
  },
  {
    path: '/therb-gh/__docusaurus/debug/routes',
    component: ComponentCreator('/therb-gh/__docusaurus/debug/routes','fa4'),
    exact: true
  },
  {
    path: '/therb-gh/blog',
    component: ComponentCreator('/therb-gh/blog','258'),
    exact: true
  },
  {
    path: '/therb-gh/blog/archive',
    component: ComponentCreator('/therb-gh/blog/archive','c78'),
    exact: true
  },
  {
    path: '/therb-gh/blog/tags',
    component: ComponentCreator('/therb-gh/blog/tags','c35'),
    exact: true
  },
  {
    path: '/therb-gh/blog/tags/becat',
    component: ComponentCreator('/therb-gh/blog/tags/becat','003'),
    exact: true
  },
  {
    path: '/therb-gh/blog/tags/hello',
    component: ComponentCreator('/therb-gh/blog/tags/hello','c52'),
    exact: true
  },
  {
    path: '/therb-gh/blog/tags/therb',
    component: ComponentCreator('/therb-gh/blog/tags/therb','c9c'),
    exact: true
  },
  {
    path: '/therb-gh/blog/welcome',
    component: ComponentCreator('/therb-gh/blog/welcome','683'),
    exact: true
  },
  {
    path: '/therb-gh/markdown-page',
    component: ComponentCreator('/therb-gh/markdown-page','325'),
    exact: true
  },
  {
    path: '/therb-gh/docs',
    component: ComponentCreator('/therb-gh/docs','fb4'),
    routes: [
      {
        path: '/therb-gh/docs/Component/Modelling',
        component: ComponentCreator('/therb-gh/docs/Component/Modelling','64c'),
        exact: true,
        'sidebar': "tutorialSidebar"
      },
      {
        path: '/therb-gh/docs/intro',
        component: ComponentCreator('/therb-gh/docs/intro','016'),
        exact: true,
        'sidebar': "tutorialSidebar"
      },
      {
        path: '/therb-gh/docs/NewHasp/HowToUse',
        component: ComponentCreator('/therb-gh/docs/NewHasp/HowToUse','cda'),
        exact: true,
        'sidebar': "tutorialSidebar"
      },
      {
        path: '/therb-gh/docs/Usage/CreateTherbModel',
        component: ComponentCreator('/therb-gh/docs/Usage/CreateTherbModel','388'),
        exact: true,
        'sidebar': "tutorialSidebar"
      },
      {
        path: '/therb-gh/docs/Usage/HowToInstall',
        component: ComponentCreator('/therb-gh/docs/Usage/HowToInstall','459'),
        exact: true,
        'sidebar': "tutorialSidebar"
      },
      {
        path: '/therb-gh/docs/Usage/RunSimulation',
        component: ComponentCreator('/therb-gh/docs/Usage/RunSimulation','2b3'),
        exact: true,
        'sidebar': "tutorialSidebar"
      },
      {
        path: '/therb-gh/docs/Usage/UploadToServer',
        component: ComponentCreator('/therb-gh/docs/Usage/UploadToServer','311'),
        exact: true,
        'sidebar': "tutorialSidebar"
      }
    ]
  },
  {
    path: '/therb-gh/',
    component: ComponentCreator('/therb-gh/','bb3'),
    exact: true
  },
  {
    path: '*',
    component: ComponentCreator('*')
  }
];
