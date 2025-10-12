install-reportgenerator:
	dotnet tool install --global dotnet-reportgenerator-globaltool

run-test:
	@echo "Clean up previous test results..."
	@rm -rf TestResults/
	@echo "Generating code coverage report..."
	@dotnet test --collect:"XPlat Code Coverage"

see-report:
	@reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:coveragereport
	open coveragereport/index.html