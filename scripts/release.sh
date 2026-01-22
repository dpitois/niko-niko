#!/bin/bash
set -e

# 1. Verification of working directory
if [ -n "$(git status --porcelain)" ]; then
  echo "Error: Working directory is not clean. Please commit or stash your changes."
  exit 1
fi

CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
VERSION=$(date +%Y.%m.%d)

echo "Starting release $VERSION..."

# 1.5 Update develop
echo "Updating develop branch..."
git checkout develop
git pull origin develop

# 2. Prepare main branch
echo "Updating main branch..."
git checkout main
git pull origin main
PREVIOUS_HEAD=$(git rev-parse HEAD)

# 3. Merge develop
echo "Merging develop into main..."
git merge develop --no-ff -m "chore(release): merge develop into main for $VERSION"

# 4. Generate Version
echo "Updating version in package.json to $VERSION..."
npm version "$VERSION" --no-git-tag-version

# 5. Generate Changelog
if [ ! -f CHANGELOG.md ]; then
  echo "Generating first CHANGELOG.md (full history)..."
  npx conventional-changelog -p angular -i CHANGELOG.md -s -r 0
else
  echo "Updating CHANGELOG.md from $PREVIOUS_HEAD..."
  # Note: conventional-changelog usually looks for tags. 
  # We use the previous HEAD as the reference point for the increment.
  npx conventional-changelog -p angular -i CHANGELOG.md -s --commit-path . --from "$PREVIOUS_HEAD"
fi

# 6. Finalize Release Commit
echo "Finalizing release commit..."
git add CHANGELOG.md package.json
# If package-lock.json exists at root, add it too
if [ -f package-lock.json ]; then
  git add package-lock.json
fi

git commit --amend --no-edit # Amending the merge commit to include changelog and version update
# Or separate commit if preferred. Amending keeps the history cleaner on main.
# Actually, better to have a dedicated release commit after the merge.
# git commit -m "chore(release): $VERSION"

echo "Release $VERSION completed successfully on branch main."
echo "Switching back to $CURRENT_BRANCH..."
git checkout "$CURRENT_BRANCH"
