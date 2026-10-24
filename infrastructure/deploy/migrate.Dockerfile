FROM mcr.microsoft.com/dotnet/runtime-deps:10.0
COPY efbundle /app/efbundle
ENTRYPOINT [ "/app/efbundle" ]