# .NETCORETASKS
# UserProfileApp

## Overview

**UserProfileApp** is an ASP.NET Core web application designed for users to manage their profiles, upload files, and utilize role-based access control (RBAC). The app includes features such as user registration, login, password reset, profile management, and file uploading. It also implements RBAC, so that users can only manage their own profiles and files, while administrators have broader access.

---

## Features

- **User Authentication**: Allows users to register, log in, and reset their passwords.
- **Role-Based Access Control (RBAC)**: Users are assigned roles like "Admin" or "User," and access to features is controlled based on the user’s role.
- **Profile Management**: Users can manage their profiles (view, update), upload profile pictures, and manage uploaded documents.
- **File Upload**: Users can upload profile pictures and documents in formats such as `.pdf`, `.docx`, and `.jpg`.
- **Password Reset via Email**: Users can request a password reset, and a reset link is sent to their email.
- **Email Notifications**: Email notifications are integrated for password resets and other important notifications.

---

## Prerequisites

Before setting up the project, ensure you have the following:

1. **.NET Core SDK 6.0 or later**:  
   Install the latest version of .NET SDK from [here](https://dotnet.microsoft.com/download).
   
2. **Visual Studio 2022 or Visual Studio Code**:  
   You can use either of these editors for developing, building, and running the project.

3. **SMTP Server or Email Provider**:  
   For email notifications (e.g., password reset emails), configure an SMTP server or use an external email provider (e.g., SendGrid).

---

## Project Structure

Here's a brief overview of the project structure:

- **`Controllers/`**: Contains the controller logic (e.g., managing user accounts, handling file uploads, etc.).
- **`Models/`**: Contains the data models for handling user profiles, authentication, and other entities.
- **`Views/`**: Contains Razor pages for rendering the user interface (UI).
- **`wwwroot/`**: Contains static files (CSS, JavaScript, images).
- **`Data/`**: Contains the in-memory database context and other application data.
- **`Program.cs`**: Configures the app’s services, middleware, and the HTTP request pipeline.

---

## Setup and Running the Application

### 1. Clone the Repository

First, clone the repository to your local machine:

```bash
git clone https://github.com/YourGitHubUsername/UserProfileApp.git
cd UserProfileApp
```

### 2. Open the Project in Visual Studio or VS Code

- Open `UserProfileApp.sln` in **Visual Studio** (recommended for .NET development).
- Alternatively, you can use **Visual Studio Code**.

### 3. In-Memory Database Configuration

This project uses an **in-memory database**, which means no external database configuration is necessary. The database is automatically configured in `Program.cs` or `Startup.cs`:

```csharp
// In Program.cs or Startup.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("UserProfileApp"));
```

All data is stored in memory while the application is running, and is reset when the application restarts.

### 4. Configure Email (SMTP) for Password Reset

If your app needs to send password reset emails, you need to configure an SMTP server or an email provider in `appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.yourserver.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@example.com",
  "SmtpPassword": "your-email-password",
  "FromEmail": "no-reply@yourapp.com"
}
```

### 5. Create Initial Roles

To implement **Role-Based Access Control (RBAC)**, you must create roles such as **Admin** and **User**. You can seed these roles in your application startup by adding the following code in `Program.cs`:

```csharp
// Add this to your Program.cs to create roles on startup
public static async Task CreateRoles(IServiceProvider serviceProvider)
{
    var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var UserManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    string[] roleNames = { "Admin", "User" };
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        var roleExist = await RoleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create an admin user
    var adminUser = new IdentityUser
    {
        UserName = "admin@yourapp.com",
        Email = "admin@yourapp.com"
    };

    string adminPassword = "Admin@123";
    var user = await UserManager.FindByEmailAsync("admin@yourapp.com");

    if (user == null)
    {
        var createAdminUser = await UserManager.CreateAsync(adminUser, adminPassword);
        if (createAdminUser.Succeeded)
        {
            await UserManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
```

### 6. Run the Application

Run the application using the following command:

```bash
dotnet run
```

Alternatively, you can run the app from Visual Studio by pressing **F5**.

Once the application starts, it will be available at `https://localhost:5001` or `http://localhost:5000` (depending on your configuration).

### 7. Access the Application

- **User Registration**: Navigate to `/Account/Register` to register a new user.
- **Admin Dashboard**: Log in as the admin (`admin@yourapp.com`, password: `Admin@123`) to access the admin features.
- **Profile Management**: Once logged in, users can update their profiles and manage their files.

---

## Running Unit Tests

To run the tests (if unit tests are set up in your project), use the following command:

```bash
dotnet test
```

This will run all tests in the solution and provide test results.

---

## Troubleshooting

- **Data Reset**: The in-memory database is volatile and resets every time the application restarts.
- **Email Issues**: Double-check your SMTP configuration if password reset emails are not working.

---

## Contributing

If you would like to contribute to this project, feel free to create a pull request or open an issue for discussion.

---

## License

This project is licensed under the MIT License.
