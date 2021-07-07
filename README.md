<h1 align="center">Markop Test</h1>
<div align="center">
    <p><a href="https://github.com/AliRezaBeigy/MarkopDev/MarkopTest/blob/master/LICENSE"><img src="https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge" alt="MIT License"></a>
    <a href="http://makeapullrequest.com"><img src="https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=for-the-badge" alt="PR&#39;s Welcome"></a>
    <img src="https://img.shields.io/github/stars/MarkopDev/MarkopTest?style=for-the-badge" alt="GitHub Repo stars"></p>
</div>

Markop Test is a free, open-source, focused testing tool for the ASP.NET framework.

## Integration Test
A part of software testing that tests individual code components to validate interactions among different software system modules. \
We use these tests are used to test the app's infrastructure and the whole framework, often including the following components:
- Database
- File system
- Network appliances
- Request-response pipeline

Markop provides an app factory abstraction to make it easy to implement a **clean** integration test and view output instead of using external API testing tools such as Postman.
<p align="center">
    <img alt="integration-test" src="assets/integration-test.png" width="1271" />
</p>

#### Usage
Let's kick things off by providing an example. First of all, you should create a project for this type of test and
create `AppFactory` class that extends **MarkopIntegrationTestFactory** class and pass web app's `Startup` class like [sample project](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/IntegrationTest/AppFactory.cs) \
Imagine you have API for getting user profile with `/api/User/GetProfile` path, create Controller folder next create User folder and after that create a `GetProfileTests` class extends your `AppFactory` into that folder like [sample project](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/IntegrationTest/Controller/User/GetProfileTests.cs) \
Write method with any name to test your API, use `Post` or `Get` method and pass data and options for send request like [sample project](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/IntegrationTest/Controller/User/GetProfileTests.cs)  \
After rebuilding the project, open `Test Explorer` or `Unit Tests` panel and run the method to check the result \
In summary, the process is as follows:
1. Create `classlib` project
2. Create `AppFactory` class that extends **MarkopIntegrationTestFactory** class and pass web app's `Startup` ([sample](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/IntegrationTest/AppFactory.cs))
3. Create `Controller/User` folder
4. Create `GetProfileTests` class extends your `AppFactory` into`Controller/User` folder ([sample](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/IntegrationTest/Controller/User/GetProfileTests.cs))
5. Write a method to send a request with API input using `Post` or `Get` method ([sample](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/IntegrationTest/Controller/User/GetProfileTests.cs))
6. Build project
7. Open `Test Explorer` or `Unit Tests` panel and run the method to check the result

## Functional Test
The test determines the product's functionality that can be done by aggregating integration tests and comparing the actual output with the predetermined output. \
We use case scenarios for functional testing. For example, in a news system, we must test managing news scenarios such as **Create**, **Edit** and **Delete** news entity, you can implement transaction workflow in your system. \
As in the integration testing, Markop provides an app factory abstraction to make it easy to implement a **clean** functional test.

#### Usage
Let's give an example to use this type of software testing. Firstly you should create a project for this type of test and add an integration test project as a reference of it then create `AppFactory` class that extends **MarkopFunctionalTestFactory** class and pass web app's `Startup` class like [sample project](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/FunctionalTest/AppFactory.cs) \
Now you can implement your scenario, create a class with any name you want, for example, ManageNews and write method and use integration test method available in the previous project like [sample project](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/FunctionalTest/Scenarios/ManageNews.cs) \
In summary, the process is as follows:
1. Create `classlib` project
2. Add integration test project as a reference of this project
3. Create `AppFactory` class that extends **MarkopFunctionalTestFactory** class and pass web app's `Startup` class ([sample](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/FunctionalTest/AppFactory.cs))
4. Create `Scenarios` folder
5. Create `ManageNews` class extends your `AppFactory` into `Scenarios` folder like ([sample](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/FunctionalTest/Scenarios/ManageNews.cs))
6. Write a method to call integration tests and pass input and compare the actual output with predetermined output ([sample](https://github.com/MarkopDev/MarkopTest/blob/master/sample/test/FunctionalTest/Scenarios/ManageNews.cs))
7. Build project
8. Open `Test Explorer` or `Unit Tests` panel and run the method to check the result

## Contributions
If you're interested in contributing to this project, first of all, We would like to extend my heartfelt gratitude. \
Please feel free to reach out to us if you need help.

## LICENSE
MIT