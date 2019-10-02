FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build

COPY . /source
WORKDIR /source/Replica.App
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/core/runtime:3.0

# Install packages
RUN apt-get update && \
    apt-get install -y libgit2-dev && \
    ln -s /usr/lib/x86_64-linux-gnu/libgit2.so /lib/x86_64-linux-gnu/libgit2-7ce88e6.so

COPY --from=build /out /app
COPY --from=build /source/.git /app/.git 

EXPOSE 3001

CMD cd /app && dotnet Replica.App.dll