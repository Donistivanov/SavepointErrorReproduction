# Savepoint Error Reproduction

```
# Update connection string in `Program.cs` and then run
dotnet ef database drop -f && dotnet ef database update --no-build --verbose
```