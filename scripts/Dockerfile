FROM mcr.microsoft.com/dotnet/aspnet:9.0
# Set the working directory
WORKDIR /app
# Copy the published output to the container
COPY . .
EXPOSE 22345
# Start the application
ENTRYPOINT ["dotnet", "UZonMailService.dll"]