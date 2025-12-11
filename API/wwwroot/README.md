# Static Files Directory

This directory (`wwwroot`) is used by ASP.NET Core to serve static files, including uploaded images.

## Structure

```
wwwroot/
└── uploads/
    └── {companyID}/
        ├── {companyID}_logo.jpg          # Company logo
        └── {ItemVariantID}/
            ├── {ItemVariantID}_thumb.jpg # Thumbnail image
            ├── {ItemVariantID}_1.jpg     # First additional image
            ├── {ItemVariantID}_2.jpg     # Second additional image
            └── {ItemVariantID}_3.jpg     # Third additional image
```

## Notes

- This directory must exist for the file upload functionality to work correctly.
- The `uploads/` subdirectory and its contents are excluded from git via `.gitignore` to prevent committing user-uploaded files.
- Subdirectories under `uploads/` are created automatically when files are uploaded.

## Related Documentation

See `/docs/IMAGE_STORAGE_STRUCTURE.md` for complete documentation on the image storage system.
