FROM mcr.microsoft.com/dotnet/aspnet:10.0
# 安装 Kerberos/GSSAPI 运行库，避免程序启动时缺少 libgssapi_krb5.so.2
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
# Set the working directory 
WORKDIR /app
# Copy the published output to the container
COPY . .
EXPOSE 22345
# Start the application
ENTRYPOINT ["dotnet", "UzonMailService.dll"]
