# ShowScraper.Api
.NET core example consuming a service and caching it using elastic

ElasticSearch was chosen in this case because:

- Elastic will provide useful features for future usage often required for this type of data, where it is likely read performance, faceted search are important. 
- The data stored is a cached version of other data, so ACID capabilities are less important 
- The future API's should be performant and preferable need no other caching layers

Docker images were chosen because:
- Has no prerequisites on the developers machine other than Docker being installed. Make sure Linux containers are active.

To start the application stack using docker compose from a powershell command:

    docker-compose up --build
    
his should give you a running instance of the ShowScraper.Api, and a 2-node ElasticSearch cluster for persistence of the cached shows from TvMaze.

You should be able to navigate to the API documentation and execute a test request:
It might take a while before the Elastic cluster is ready for querying:

    http://127.0.0.1/swagger/index.html


To run the API outside a docker container:

 	cd .\ShowScraper.Api 
 	dotnet run
 	exec  http://127.0.0.1:5000/swagger/index.html

At this point there is no data available yet, run these commands in a new powershell to start indexing. This will require you to have the .NET Core 2.2 SDK installed locally (does not use docker compose yet):

    cd .\ShowScraper.Indexing 
    dotnet run [optional: --startPage 10]

Integration tests written based on XUnit can be run from ShowScraper.Tests (requires the "docker-compose" window still to be up and running):
    cd .\ShowScraper.Tests
    dotnet test

Open points and considerations:
- Testing and the Indexing application are not containerized yet, they still require local installations of .NET Core
- Settings and command line parameters are not configurable yet. As this will not be deployed to other environments this didn't have priority
- Tests and Indexing application are not containerized yet
- The composition of the classes and assemblies is not ideal yet, to enable better unit testing some interfaces will have to be extracted
- Only integration tests exist, mocking should still be added to create proper isolated unit tests
- Initially I wanted to scrape and index asynchronously based on the producer/consumer model in GoLang's Channels, but this was too much work for now 
- I don't like the property naming in the models used for scraping the API and persisting in elastic
