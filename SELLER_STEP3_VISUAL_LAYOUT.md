# Visual Layout Comparison - Product Step 3 Media Section

## BEFORE (Old Layout)

```
┌─────────────────────────────────────────────────────────────┐
│ Images                                                       │
│ ┌─────────────────────────┬─────────────────────────────┐  │
│ │ Thumbnail               │ Product Images              │  │
│ │ ┌─────────────────────┐ │ ┌─────────────────────────┐ │  │
│ │ │ Choose Image        │ │ │ Choose Images           │ │  │
│ │ └─────────────────────┘ │ └─────────────────────────┘ │  │
│ │                         │                             │  │
│ │ [Thumbnail Preview]     │ "3 images" text only        │  │
│ │ (if uploaded)           │ (no visual previews)        │  │
│ └─────────────────────────┴─────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

**Issues:**
- Single "Images" label for both thumbnail and product images
- Vertical stacking within horizontal columns
- No way to remove individual images
- No way to reorder images
- No visual preview of product images (just count)
- No video support


## AFTER (New Layout)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Thumbnail │ Choose Image │ ┌──────┐                                     │
│           │              │ │  ×   │ [60x60 preview]                     │
│           │              │ └──────┘                                     │
├─────────────────────────────────────────────────────────────────────────┤
│ Product   │ Choose       │ ┌──────┐  ┌──────┐  ┌──────┐                │
│ Images    │ Images       │ │  ×   │  │  ×   │  │  ×   │                │
│           │              │ │[60x60]│  │[60x60]│  │[60x60]│              │
│           │              │ │ ←  → │  │ ←  → │  │  ←   │                │
│           │              │ └──────┘  └──────┘  └──────┘                │
└─────────────────────────────────────────────────────────────────────────┘

Note: Video section is currently disabled pending backend support.
```

**Improvements:**
✅ Separate label for each media type (Thumbnail, Product Images, Video)
✅ Horizontal row layout: Label | Button | Preview
✅ Individual remove button (×) on each image/video
✅ Reorder buttons (← →) on each product image
✅ Visual previews of all product images (not just count)
✅ Video upload and preview support
✅ All media same size (60x60) for consistency (except video: 120x90)


## Detailed Feature Breakdown

### 1. Thumbnail Row
```
Label: "Thumbnail"
Button: "Choose Image" (single file selection)
Preview: 60x60px square image
Actions: Remove button (×) in top-right corner
```

### 2. Product Images Row
```
Label: "Product Images"
Button: "Choose Images" (multiple file selection, max 10)
Preview: Horizontal grid of 60x60px squares
Actions per image:
  - Remove button (×) in top-right corner of image
  - Left arrow (←) - moves image left (bottom bar, hidden on first image)
  - Right arrow (→) - moves image right (bottom bar, hidden on last image)
```

### 3. Video Row
```
Currently disabled pending backend video upload support.
When enabled:
  Label: "Video"
  Button: "Choose Video" (single file selection)
  Preview: 120x90px video element with native controls
  Actions: Remove button (×) in top-right corner
```


## Button Styles

### Choose Button
```
Background: #3498db (blue)
Hover: #2980b9 (darker blue)
Padding: 8px 16px
Border-radius: 4px
```

### Remove Button (×)
```
Shape: Circle
Background: #e74c3c (red)
Hover: #c0392b (darker red)
Size: 20x20px
Position: Absolute top-right with -8px offset
Border: 2px white
```

### Move Buttons (← →)
```
Background: #3498db (blue)
Hover: #2980b9 (darker blue)
Size: 20x20px
Position: Bottom-center in semi-transparent white container
```


## Mobile Responsive Behavior

On screens < 768px:

```
┌────────────────────┐
│ Thumbnail          │
│ ┌────────────────┐ │
│ │ Choose Image   │ │
│ └────────────────┘ │
│                    │
│ ┌──────┐           │
│ │  ×   │           │
│ └──────┘           │
├────────────────────┤
│ Product Images     │
│ ┌────────────────┐ │
│ │ Choose Images  │ │
│ └────────────────┘ │
│                    │
│ [Grid of previews] │
├────────────────────┤
│ Video              │
│ ┌────────────────┐ │
│ │ Choose Video   │ │
│ └────────────────┘ │
│                    │
│ [Video preview]    │
└────────────────────┘
```

**Mobile Changes:**
- Stack vertically instead of horizontal row
- Label above button and preview
- Preview area full width
- Maintains same functionality
