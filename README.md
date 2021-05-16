# MrMeeseeks.ResXTranslator

## Features

- choose one of the supported languages from DeepL as default language
- let DeepL translate the rest automatically
- and let language experts override specific localizations

## Sample workflow

```yml
name: Create translation pull request

# Controls when the action will run. Triggers the workflow on push or pull request
# events but only if resx files where involved
on:
  push:
    paths:
    - '**.resx'

# GitHub automatically creates a GITHUB_TOKEN secret to use in your workflow.
env:
  GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v2

      # Translate and combine ResX files
      - name: Translation and Combination
        id: translator
        uses: Yeah69/MrMeeseeks.ResXTranslationCombinator@v1.0.0
        with:
          # Take root directory of the repository as directory to search for the ResX files
          dir: ${{ './' }}
          # The authentication key of the DeepL API access
          auth: ${{ secrets.DEEPL_API_AUTH_KEY }}

      - name: create-pull-request
        uses: peter-evans/create-pull-request@v3.9.2
        with:
          title: '${{ steps.translator.outputs.summary-title }}'
          commit-message: '${{ steps.translator.outputs.summary-details }}'
```

## References

- https://github.com/dotnet/samples/tree/main/github-actions/DotNet.GitHubAction
  - Took that sample as a template for this project
  - https://docs.microsoft.com/en-us/dotnet/devops/create-dotnet-github-action the corresponding tutorial which teaches how to implement a Github action with .Net
- https://github.com/IEvangelist/resource-translator
  - Similar project to this one. I didn't have a detailed look into it, so I maybe am inaccurate, but I think these are the differences:
   - Uses Azure instead of DeepL for translations
   - Has no feature to override the automatic translation (from what I saw, I didn't found it)
   - Is written in TypeScript (just a fact; shouldn't matter much if you just want to use it)
 - https://github.com/marketplace/actions/machine-translator market place entry
