# Security
ASP.NET Core 8 MVC. The application focuses on sending encrypted messages using the Triple DES (symmetric key) and RSA (asymmetric key) algorithms.
# ASP.NET Core MVC Secure Web Application Project Documentation

## Introduction

This document aims to provide a detailed and comprehensive explanation of a web application developed using ASP.NET Core 8 MVC. The application focuses on providing a secure system for user authentication and encrypted message exchange. This application was built to meet specific requirements, including user registration, password hashing using SHA-256, storing data in text files, password reset, sending encrypted messages using Triple DES (symmetric key) and RSA (asymmetric key), and logout functionality.

The code is designed to be as simple as possible while maintaining the required functionality. This documentation aims to help you understand each part of the code step by step.

## Technologies Used

* **ASP.NET Core 8 MVC:** A framework for developing modern web applications.
* **C#:** The primary programming language used.
* **HTML, CSS, JavaScript (Bootstrap):** For front-end technologies.
* **Text File Storage:** For storing user data, messages, and keys.
* **SHA-256:** For password hashes.
* **Triple DES:** For symmetric message encryption.
* **RSA (2048-bit):** For asymmetric message encryption.

---

## 1. Setting Up and Running the Project

### 1.1. Prerequisites

To build and run this project, you will need the following tools installed on your system:

* **.NET 8 SDK:** You can download it from [the official Microsoft website](https://dotnet.microsoft.com/download/dotnet/8.0).

### 1.2. Project Structure

The project consists of the following main folders and files under the root folder `NetMvcApp`:

* **`/NetMvcApp.csproj`**: The project file containing the build settings and dependencies.
* **`/Program.cs`**: The main entry point for the application, where services and the HTTP pipeline are configured.
* **`/appsettings.json`**: A file for configuring application settings.
* **`/Controllers`**: Contains the controllers that handle user requests and coordinate responses.
* `AccountController.cs`: Responsible for user registration, login, password reset, password change, and logout.
* `MessageController.cs`: Responsible for sending encrypted messages (Triple DES and RSA).
* **`/Models`**: Contains data models representing the data used in the application and view models.
* `User.cs`: Represents user data.
* `RegisterViewModel.cs`, `LoginViewModel.cs`, `ForgotPasswordViewModel.cs`, `VerifyPhoneNumberViewModel.cs`, `ResetPasswordViewModel.cs`, `ChangePasswordViewModel.cs`: View models for account operations.
* `SendMessageViewModel.cs`: View model for sending messages.
* **`/Views`**: Contains the view files (Views) representing the user interface (HTML).
* **`/Account`**: Contains views related to account operations (registration, login, etc.).
* **`/Message`**: Contains views related to sending messages.
* **`/Shared`**: Contains shared views such as the main layout (`_Layout.cshtml`).
* `_ViewImports.cshtml`: For importing common namespaces into views.
* **`/Services`**: Contains business services that encapsulate specific logic.
* `FileStorageService.cs`: Responsible for reading and writing data to and from text files.
* `EncryptionService.cs`: Responsible for hashing, encryption, and decryption operations.
* **`/Data`**: This folder (created at launch if it does not exist) will contain text files that store application data:
* `users.txt`: For storing user information (including the RSA public key).
* `rsa_keys.txt`: For storing users' RSA private keys.
* `messages_tdes.txt`: For storing messages encrypted using Triple DES.
* `tdes_keys.txt`: For storing Triple DES keys and the corresponding message initialization vectors (IVs).
* `messages_rsa.txt`: For storing RSA-encrypted messages.
* **`/wwwroot`**: Contains static files such as CSS, JavaScript, and images.

### 1.3. Building and Running the Application

1. **Open a terminal or command prompt.**
2. **Navigate to the root folder of the `NetMvcApp` project: `cd path/to/NetMvcApp`
3. **Build the project**: Execute the following command:
```bash
dotnet build
```
4. **Run the project**: Execute the following command:
```bash
dotnet run
```
5. **Accessing the application**: Open a web browser and navigate to the address displayed in the terminal (usually `https://localhost:xxxx` or `http://localhost:xxxx`, where `xxxx` is the port number).

### 1.4. A Note on Invariant Globalization

While developing this project in a limited environment, we encountered a problem related to the lack of the ICU (International Components for Unicode) library, which is essential for globalization operations in .NET. As a workaround, we enabled "static globalization" mode in the project file (`NetMvcApp.csproj`) by adding the following line under `<PropertyGroup>`:

```xml
<InvariantGlobalization>true</InvariantGlobalization>
```

This setting allows the application to run without relying entirely on the ICU libraries. If you are running the project on a system with the full .NET SDK installed with all its dependencies (including ICU), you may not need this setting and can remove it if desired. However, if you encounter ICU-related errors while building or running on your system, this setting provides a workaround.

The main effect of setting globalization to constant is that the application will treat all cultures in a uniform (static) way, which may affect                                # ASP.NET Core MVC Secure Web Application Project Documentation

## Introduction

This document aims to provide a detailed and comprehensive explanation of a web application developed using ASP.NET Core 8 MVC. The application focuses on providing a secure system for user authentication and encrypted message exchange. This application was built to meet specific requirements, including user registration, password hashing using SHA-256, storing data in text files, password reset, sending encrypted messages using Triple DES (symmetric key) and RSA (asymmetric key), and logout functionality.

The code is designed to be as simple as possible while maintaining the required functionality. This documentation aims to help you understand each part of the code step by step.

## Technologies Used

* **ASP.NET Core 8 MVC:** A framework for developing modern web applications.
* **C#:** The primary programming language used.
* **HTML, CSS, JavaScript (Bootstrap):** For front-end technologies.
* **Text File Storage:** For storing user data, messages, and keys.
* **SHA-256:** For password hashes.
* **Triple DES:** For symmetric message encryption.
* **RSA (2048-bit):** For asymmetric message encryption.

---

## 1. Setting Up and Running the Project

### 1.1. Prerequisites

To build and run this project, you will need the following tools installed on your system:

* **.NET 8 SDK:** You can download it from [the official Microsoft website](https://dotnet.microsoft.com/download/dotnet/8.0).

### 1.2. Project Structure

The project consists of the following main folders and files under the root folder `NetMvcApp`:

* **`/NetMvcApp.csproj`**: The project file containing the build settings and dependencies.
* **`/Program.cs`**: The main entry point for the application, where services and the HTTP pipeline are configured.
* **`/appsettings.json`**: A file for configuring application settings.
* **`/Controllers`**: Contains the controllers that handle user requests and coordinate responses.
* `AccountController.cs`: Responsible for user registration, login, password reset, password change, and logout.
* `MessageController.cs`: Responsible for sending encrypted messages (Triple DES and RSA).
* **`/Models`**: Contains data models representing the data used in the application and view models.
* `User.cs`: Represents user data.
* `RegisterViewModel.cs`, `LoginViewModel.cs`, `ForgotPasswordViewModel.cs`, `VerifyPhoneNumberViewModel.cs`, `ResetPasswordViewModel.cs`, `ChangePasswordViewModel.cs`: View models for account operations.
* `SendMessageViewModel.cs`: View model for sending messages.
* **`/Views`**: Contains the view files (Views) representing the user interface (HTML).
* **`/Account`**: Contains views related to account operations (registration, login, etc.).
* **`/Message`**: Contains views related to sending messages.
* **`/Shared`**: Contains shared views such as the main layout (`_Layout.cshtml`).
* `_ViewImports.cshtml`: For importing common namespaces into views.
* **`/Services`**: Contains business services that encapsulate specific logic.
* `FileStorageService.cs`: Responsible for reading and writing data to and from text files.
* `EncryptionService.cs`: Responsible for hashing, encryption, and decryption operations.
* **`/Data`**: This folder (created at launch if it does not exist) will contain text files that store application data:
* `users.txt`: For storing user information (including the RSA public key).
* `rsa_keys.txt`: For storing users' RSA private keys.
* `messages_tdes.txt`: For storing messages encrypted using Triple DES.
* `tdes_keys.txt`: For storing Triple DES keys and the corresponding message initialization vectors (IVs).
* `messages_rsa.txt`: For storing RSA-encrypted messages.
* **`/wwwroot`**: Contains static files such as CSS, JavaScript, and images.

### 1.3. Building and Running the Application

1. **Open a terminal or command prompt.**
2. **Navigate to the root folder of the `NetMvcApp` project: `cd path/to/NetMvcApp`
3. **Build the project**: Execute the following command:
```bash
dotnet build
```
4. **Run the project**: Execute the following command:
```bash
dotnet run
```
5. **Accessing the application**: Open a web browser and navigate to the address displayed in the terminal (usually `https://localhost:xxxx` or `http://localhost:xxxx`, where `xxxx` is the port number).

### 1.4. A Note on Invariant Globalization

While developing this project in a limited environment, we encountered a problem related to the lack of the ICU (International Components for Unicode) library, which is essential for globalization operations in .NET. As a workaround, we enabled "static globalization" mode in the project file (`NetMvcApp.csproj`) by adding the following line under `<PropertyGroup>`:

```xml
<InvariantGlobalization>true</InvariantGlobalization>
```

This setting allows the application to run without relying entirely on the ICU libraries. If you are running the project on a system with the full .NET SDK installed with all its dependencies (including ICU), you may not need this setting and can remove it if desired. However, if you encounter ICU-related errors while building or running on your system, this setting provides a workaround.

The main effect of setting globalization to constant is that the application will treat all cultures in a uniform (static) way, which may affect              ## 2. Detailed Code Structure

Let's delve into the details of each component of the project.

### 2.1. Models

The `Models` folder contains C# classes that represent the application's core data (data entities) as well as the models used by views (View Models) to transfer data between controllers and views.

#### 2.1.1. `User.cs`

This model represents the basic user data.

```csharp
namespace NetMvcApp.Models
{
public class User
{
public int UserId { get; set; }
public string Name { get; set; }
public string Email { get; set; }
public string HashedPassword { get; set; }
public string PhoneNumber { get; set; }
public string RSAPublicKey { get; set; } // User's RSA public key (Base64 encoded)
}
}
```

* `UserId`: A unique identifier for each user (an integer).
* `Name`: The user's name.
* `Email`: The user's email address (used for logging in).
* `HashedPassword`: The password hashed using SHA-256.
* `PhoneNumber`: The user's phone number (used for password reset).
* `RSAPublicKey`: The user's RSA public key, stored as a Base64-encoded string. It is generated when the user registers and is used to encrypt messages sent to them using RSA.

#### 2.1.2. Account View Models

These models are specifically used to transfer data to and from account view models (registration, login, etc.) and typically include data annotations.

* **`RegisterViewModel.cs`**: For the new registration page.
```csharp
using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
public class RegisterViewModel
{
[Required(ErrorMessage = "Username required")]
[Display(Name = "Full Name")]
public string Name { get; set; }

[Required(ErrorMessage = "Email required")]
[EmailAddress(ErrorMessage = "Invalid email format")]
[Display(Name = "Email")]
public string Email { get; set; }

[Required(ErrorMessage = "Phone number required")]
[Phone(ErrorMessage = "Invalid phone number format")]
[Display(Name = "Phone number")]
public string PhoneNumber { get; set; }

[Required(ErrorMessage = "Password required")]
[DataType(DataType.Password)]
[StringLength(100, ErrorMessage = "Password must be at least 6 characters long.", MinimumLength = 6)]
[Display(Name = "Password")]
public string Password { get; set; }

[DataType(DataType.Password)]
[Display(Name = "Confirm Password")]
[Compare("Password", ErrorMessage = "The password and confirmation do not match.")]

public string ConfirmPassword { get; set; }
}
}
```
Contains fields for Name, Email, Phone Number, Password, and Confirm Password, with appropriate validation rules.

* **`LoginViewModel.cs`**: For the login page.
```csharp
using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
public class LoginViewModel
{
[Required(ErrorMessage = "Email required")]
[EmailAddress(ErrorMessage = "Invalid email format")]
[Display(Name = "Email")]
public string Email { get; set; }

[Required(ErrorMessage = "Password required")]
[DataType(DataType.Password)]
[Display(Name = "Password")]
public string Password { get; set; }
}
}
```
Contains the email and password fields.

* **`ForgotPasswordViewModel.cs`**: For the password reset request page (email input).
```csharp
using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
public class ForgotPasswordViewModel
{
[Required(ErrorMessage = "Email address is required to initiate the reset process.")]
[EmailAddress(ErrorMessage = "Invalid email format.")]
[Display(Name = "Registered Email")]
public string Email { get; set; }
}
}
```

* **`VerifyPhoneNumberViewModel.cs`**: For the phone number verification page during password reset.
```csharp
using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
public class VerifyPhoneNumberViewModel
{
[Required]
public string Email { get; set; } // Passed hidden from the previous step

[Required(ErrorMessage = "Phone number required for verification.")]
[Phone(ErrorMessage = "Invalid phone number format.")]
[Display(Name = "Registered phone number")]
public string PhoneNumber { get; set; }
}
}
```

* **`ResetPasswordViewModel.cs`**: For the new password entry page after email and phone number verification.
```csharp
using System.ComponentModel.DataAnnotations;

namespace NetMvcA
