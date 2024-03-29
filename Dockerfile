# Set the base image as the .NET 6.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./
RUN dotnet publish ./Action/Action.csproj -c Release -o . --no-self-contained

# Label the container
LABEL maintainer="Dieter 'Dima' Enns <dieter.enns@gmail.com>"
LABEL repository="https://github.com/Yeah69/MrMeeseeks.ResXTranslationCombinator"
LABEL homepage="https://github.com/Yeah69/MrMeeseeks.ResXTranslationCombinator"

# Label as GitHub action
LABEL com.github.actions.name="ResX Translation and Combination"
LABEL com.github.actions.description="A Github action that searches for default, automatically translated, and manually overriden ResX files, automatically translates missing texts, and combines them to directly usable ResX files."
LABEL com.github.actions.icon="file-plus"
LABEL com.github.actions.color="purple"

ENTRYPOINT [ "dotnet", "/Action/bin/Release/net6.0/MrMeeseeks.ResXTranslationCombinator.Action.dll" ]
