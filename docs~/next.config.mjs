import { createMDX } from 'fumadocs-mdx/next';
import { achNextConfig } from 'ach-fumadocs-theme/next';

const withMDX = createMDX();

export default withMDX(achNextConfig({ repo: 'lite-db' }));
