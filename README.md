<h1 align="center">Markop Test</h1>
<h6 align="center">

[![NuGet](http://img.shields.io/nuget/v/MarkopTest.svg?label=NuGet)](https://www.nuget.org/packages/MarkopTest/)
[![Nuget](https://img.shields.io/nuget/dt/MarkopTest.svg)](https://nuget.org/packages/MarkopTest)
[![MIT License](https://img.shields.io/github/license/MarkopDev/MarkopTest.svg?color=yellow)](https://github.com/MarkopDev/MarkopTest/LICENSE.txt)
[![GitHub stars](https://img.shields.io/github/stars/MarkopDev/MarkopTest.svg?color=red)](https://github.com/MarkopDev/MarkopTest/stargazers)

</h6>

Markop Test is a free, open-source, focused testing tool for **.Net**. Using Markop Tests you can easily write **Unit tests**, **Integration tests**, **Functional tests** and **Load** tests.

## Getting Started

In order to use Markop test you should do the following steps:

1- Create a test project inside your solution (project type is class library)<br/>

<img alt="test-project-structure" src="assets/test-project-structure.png"  width="1271" /><br/>

2- Inside the newly created project add a reference to the project you want to test<br/>

<img alt="reference" src="assets/reference.png"  width="1271" /><br/>

3- Install the Markop Test package inside your test project:<br/>
 <p>Nuget:</p>
<div class="snippet-clipboard-content notranslate position-relative overflow-auto" data-snippet-clipboard-copy-content="Install-Package MarkopTest">
    <pre class="notranslate"><code>Install-Package MarkopTest</code></pre>
</div>
<p> Or using dotnet CLI:</p>
<div class="snippet-clipboard-content notranslate position-relative overflow-auto" data-snippet-clipboard-copy-content="dotnet add package MarkopTest">
    <pre class="notranslate"><code>dotnet add package MarkopTest</code></pre>
</div>

4- Depending on the kind of testing you want to perform you should go to: [Unit test](#unit-test)
, [Integration test](#integration-test),
[Functional tests](#functional-test) or load testing

## Unit Test

<p> Unit Tests are supposed to test the behaviour of a smallest piece of code. Markop Test is here to make this process fully automated. Writing Unit Tests has never been easier before!!</p>
<p>First of all you should create an <code>AppFactory</code> class extend it from <code>UnitTestFactory</code> class.</p>
<p>Then you need to override <code>Initializer</code> and <code>ConfigureTestServices</code> methods</p>
<p><code>Initializer</code> method gives you the ability to initiate a custom database for testing all you have to do is to build your custom initializer and called it here. Markop Test will take care of the rest!!</p>
<p><code>ConfigureTestServices</code> method gives you the ability to register/remove the services. This way you will have full control over the registered services of your app before you start testing!! </p>
A sample implementation looks like this:
<img alt="unit-test-app-factory" src="assets/unit-test-app-factory.png"  width="1271" /><br/>
<p>Then you need to create a class and extend it from your own <code>AppFactory</code> class </p>
<p>Next, you should define a method inside your class and put your pereferable test attribute. It can be <code>[Fact]</code> or <code>[Theory]</code> or any other valid test attribute in Xunit.</p>
<p>Now you can start writing your test code inside your method!!</p>
<p>For example in the below code we wrote code to test an extention method called <code>EmailNormalize</code></p>
<img alt="email-normalize" src="assets/email-normalize.png"  width="1271" /><br/>

## Integration Test

A part of software testing that tests individual code components to validate interactions among different software
system modules. \
We use these tests are used to test the app's infrastructure and the whole framework, often including the following
components:

- Database
- File system
- Request-response pipeline

Markop provides an app factory abstraction to make it easy to implement a **clean** integration test and view output
instead of using external API testing tools such as Postman.
<p align="center">
    <img alt="integration-test" src="assets/integration-test.png" width="1271" />
</p>

#### Usage

- TODO

## Functional Test

The test determines the product's functionality that can be done by aggregating integration tests and comparing the
actual output with the predetermined output. \
We use case scenarios for functional testing. For example, in a news system, we must test managing news scenarios such
as **Create**, **Edit** and **Delete** news entity, you can implement transaction workflow in your system. \
As in the integration testing, Markop provides an app factory abstraction to make it easy to implement a **clean**
functional test.

#### Usage

- TODO

## Contributions

If you're interested in contributing to this project, first of all, We would like to extend my heartfelt gratitude. \
Please feel free to reach out to us if you need help.

## LICENSE

MIT