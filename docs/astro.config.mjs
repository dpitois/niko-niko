// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	base: '/docs',
	trailingSlash: 'always',
	integrations: [
		starlight({
			title: 'Niko Niko Calendar',
			defaultLocale: 'en',
			locales: {
				en: {
					label: 'English',
					lang: 'en',
				},
				fr: {
					label: 'Français',
					lang: 'fr',
				},
			},
			social: [
				{ icon: 'github', label: 'GitHub', href: 'https://github.com/dpitois/niko-niko' },
			],
			sidebar: [
				{
					label: 'Guides',
					translations: {
						fr: 'Guides',
					},
					items: [
						{ label: 'User Guide', slug: 'user-guide', translations: { fr: 'Guide Utilisateur' } },
						{ label: 'Team Admin Guide', slug: 'team-admin-guide', translations: { fr: 'Guide Admin Équipe' } },
						{ label: 'Super Admin Guide', slug: 'super-admin-guide', translations: { fr: 'Guide Super Admin' } },
					],
				},
				{
					label: 'API Reference',
					translations: {
						fr: 'Référence API',
					},
					slug: 'api-reference',
				},
			],
		}),
	],
});
