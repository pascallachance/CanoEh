# Flag Images

This directory contains local copies of flag images used in the application.

## Current Implementation

Due to network restrictions in the build environment, the original PNG images from the Wikipedia URLs could not be downloaded automatically:
- Quebec flag: https://upload.wikimedia.org/wikipedia/commons/thumb/5/5f/Flag_of_Quebec.svg/960px-Flag_of_Quebec.svg.png?20250902230651
- Canada flag: https://upload.wikimedia.org/wikipedia/en/thumb/c/cf/Flag_of_Canada.svg/960px-Flag_of_Canada.svg.png?20190402205958

## Temporary SVG Files

Currently using SVG versions:
- `flag-canada.svg` - Canadian flag (red maple leaf on white with red borders)
- `flag-quebec.svg` - Quebec flag (white cross on blue with fleur-de-lis)

## To Use Original PNG Files

If you want to use the original PNG files from Wikipedia:
1. Download the images from the URLs above
2. Save them as `flag-canada.png` and `flag-quebec.png` in this directory
3. Update `src/components/FlagIcon.tsx` to reference the .png files instead of .svg files
