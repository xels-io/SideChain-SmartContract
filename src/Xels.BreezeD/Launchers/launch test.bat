cd ..
start cmd /k dotnet run -testnet -debug=all -loglevel=debug
timeout 21
start cmd /k dotnet run Xels -testnet -debug=all -loglevel=debug