FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY ./app/. .
RUN apt-get update && \
    apt-get install -y curl unzip build-essential && \
    mkdir bin && \
    cd bin && \
    curl -LO https://github.com/bitwarden/sdk/releases/download/bws-v0.3.0/bws-x86_64-unknown-linux-gnu-0.3.0.zip && \
    unzip bws-x86_64-unknown-linux-gnu-0.3.0.zip && \
    chmod +x bws && \
    mv ./bws /usr/bin/
ENTRYPOINT ["dotnet", "Api.dll"]