#!/bin/bash

# Configuration
ARCHIVE_DIR="plans/archive"
DAYS_THRESHOLD=3

# Create archive directory if it doesn't exist
mkdir -p "$ARCHIVE_DIR"

# Calculate the threshold date (YYYYMMDD)
# Works on Linux (GNU date) and macOS (BSD date)
if date --version >/dev/null 2>&1; then
    # GNU date
    THRESHOLD_DATE=$(date -d "-$DAYS_THRESHOLD days" +%Y%m%d)
else
    # BSD date (macOS)
    THRESHOLD_DATE=$(date -v-${DAYS_THRESHOLD}d +%Y%m%d)
fi

echo "Archiving plans older than $THRESHOLD_DATE..."

# Iterate over files matching the specific pattern
# Pattern: YYYYMMDD-HHmm--feature_name.md
# We accept both old format (YYYYMMDD--) and new format (YYYYMMDD-HHmm--)
# But strict regex matching inside the loop is safer.

for file in plans/*.md; do
    # Skip if it's not a file
    [ -f "$file" ] || continue

    filename=$(basename "$file")

    # Extract date part. 
    # Try to match YYYYMMDD at the start.
    if [[ $filename =~ ^([0-9]{8}) ]]; then
        file_date=${BASH_REMATCH[1]}
        
        # Compare dates
        if [ "$file_date" -lt "$THRESHOLD_DATE" ]; then
            echo "Archiving $filename ($file_date < $THRESHOLD_DATE)"
            
            # Check if file is tracked by git
            if git ls-files --error-unmatch "$file" >/dev/null 2>&1; then
                git mv "$file" "$ARCHIVE_DIR/"
            else
                mv "$file" "$ARCHIVE_DIR/"
            fi
        else
            echo "Skipping $filename (Newer or equal)"
        fi
    else
        echo "Skipping $filename (No date prefix match)"
    fi
done

echo "Cleanup complete."
