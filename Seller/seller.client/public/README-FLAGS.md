# Flag Images

This directory contains local copies of flag images used in the application.

## Current Implementation

Due to network restrictions in the build environment, the original PNG images from the Wikipedia URLs could not be downloaded automatically:
- Quebec flag: https://upload.wikimedia.org/wikipedia/commons/thumb/5/5f/Flag_of_Quebec.svg/960px-Flag_of_Quebec.svg.png?20250902230651
- Canada flag: https://upload.wikimedia.org/wikipedia/en/thumb/c/cf/Flag_of_Canada.svg/960px-Flag_of_Canada.svg.png?20190402205958

## Current SVG Files

SVG versions are now available and correctly named:
- `flag-canada.svg` - Canadian flag (red maple leaf on white with red borders) - Used for English (EN)
- `flag-quebec.svg` - Quebec flag (white cross on blue with fleur-de-lis) - Used for French (FR)

These files are referenced in `src/components/FlagIcon.tsx` and will be displayed automatically. 
The component includes inline SVG fallbacks that will only show if the files fail to load.

## File Verification

To verify the file contents:
- Canadian flag has red (#f00) and white (#fff) colors with a maple leaf shape
- Quebec flag has blue (#003F87) background with white cross and fleur-de-lis symbols

## Troubleshooting

If the old inline SVG icons are still showing:
1. Hard refresh your browser (Ctrl+F5 or Cmd+Shift+R) to clear cache
2. Verify files exist in the `public/` directory
3. Check browser console for any 404 errors
4. Restart the dev server: `npm run dev -- --force`

## To Use Original PNG Files

If you want to use the original PNG files from Wikipedia:
1. Download the images from the URLs above
2. Save them as `flag-canada.png` and `flag-quebec.png` in this directory
3. Update `src/components/FlagIcon.tsx` to reference the .png files instead of .svg files
