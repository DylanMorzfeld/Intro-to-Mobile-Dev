# 💪 FitTrack — Personal Fitness Tracker

A .NET MAUI app for tracking workouts, meals, and fitness goals, built with the MVVM pattern for Intro to Mobile Development.

---

## 📱 What It Does

| Screen | What you can do |
|---|---|
| **Workout Log** | Log workouts (type, duration, intensity, calories). Swipe right to mark one complete or edit it. Swipe left to delete. |
| **Diet Tracker** | Log meals with calories, protein, carbs, and fat. Tap a meal to expand its nutrition detail. Swipe right to edit, left to delete. See your daily total against a calorie goal. |
| **Goals** | Set weight loss, muscle gain, or endurance goals. The progress bar changes color as you get closer, and hitting a goal shows a celebration message. A Recent Activity card shows what you've been doing. |

---

## 🏗️ How It's Built (MVVM)

- **Models**: plain data (`Workout`, `Meal`, `FitnessGoal`).
- **Services**: save and load data behind interfaces (`IWorkoutRepository`, `IDietRepository`, `IGoalRepository`). Today they use local SQLite, but a real backend could replace them without touching the rest of the app.
- **ViewModels**: the logic for each screen, using CommunityToolkit.Mvvm for properties and commands.
- **Views**: XAML pages that only bind to their ViewModel.
- **Converters**: display-only logic, like turning goal numbers into a progress value or color.

---

## ✋ Gestures

- **Swipe right on a workout** to mark it complete (or undo) or edit it.
- **Tap a meal** to expand or collapse its protein, carbs, and fat.
- **Swipe left** on any item to delete it, with a confirmation dialog.

---

## 📣 Custom Events

The app uses `WeakReferenceMessenger` so ViewModels can announce things without knowing who is listening:

- `WorkoutLoggedMessage` is sent when a workout is completed, reopened, or opened for editing.
- `NutritionalDetailsRequestedMessage` is sent when a meal's nutrition detail is expanded.

An `ActivityFeedService` listens for both and feeds the Recent Activity card on the Goals page.

---

## 🎨 Resources

- **Static resources**: one color palette in `Colors.xaml` (`ActionColor`, `DangerColor`, `SuccessColor`) is used for buttons and swipe actions across all three pages.
- **Dynamic, progress-based visuals**: goal progress bars turn red, orange, or green depending on how close the goal is to completion. The colors are looked up from the app's resource dictionary at runtime.

---

## 🧭 Navigation

Three tabs (**Workouts**, **Diet**, **Goals**) built on .NET MAUI Shell. Detail pages open with `Shell.Current.GoToAsync`, passing an Id to edit an existing entry.

---

## 💾 Data

Saved locally with SQLite (`sqlite-net-pcl`), so data persists between launches.

---

## 🔮 What's Next

- Real reminders for recurring workouts (currently just a stored flag)
- Backend/API support instead of local-only storage

---

## 🛠️ Built With

.NET MAUI · CommunityToolkit.Mvvm · SQLite (sqlite-net-pcl)

---

## 🌐 Semester API (Part 1): FitTrack.Api

An ASP.NET Core Web API (.NET 10) that will become the back end for the FitTrack MAUI app. Part 1 stores data in a JSON file. Primary resource: **workouts**.

### Endpoints

| Method | URL | What it does | Success | Failure |
|---|---|---|---|---|
| GET | `/api/workouts` | List all workouts. Optional filter: `?type=Running` | 200 | none |
| GET | `/api/workouts/{id}` | Get one workout by id | 200 | 404 |
| POST | `/api/workouts` | Create a workout | 201 + `Location` header | 400, 409 |
| PUT | `/api/workouts/{id}` | Replace an existing workout | 204 | 400, 404, 409 |
| DELETE | `/api/workouts/{id}` | Delete a workout | 204 | 404 |

### Fields

| Field | Who sets it | Notes |
|---|---|---|
| `date` | Client | Date of the workout |
| `type` | Client | Activity type, e.g. Running, Walking, Cycling, WeightLifting, Swimming, Yoga, HIIT, Other |
| `durationMinutes` | Client | Must be greater than 0 |
| `caloriesBurned` | Client | Cannot be negative |
| `intensity` | Client | Low, Moderate, or High |
| `notes` | Client | Optional |
| `id` | **Server** | Assigned by the repository |
| `createdUtc` | **Server** | Set when the workout is created |

### Business rule

Only one workout of the same `type` is allowed per day. Creating or updating a workout that would break this returns **409 Conflict**. When updating, the workout being edited is not counted as its own duplicate.

### Error format

Errors use the standard ASP.NET Core problem-details format (`application/problem+json`).

### Project layout

| Folder | What it holds |
|---|---|
| `Controllers` | `WorkoutsController`: reads the request, calls the service once, returns the status code |
| `Services` | `IWorkoutService` and `WorkoutService`: the one-workout-per-type-per-day rule, reported as an outcome (not an HTTP code) |
| `Repositories` | `IWorkoutRepository` and `JsonWorkoutRepository`: reads and writes the JSON file |
| `Models` | `Workout`, the stored shape, including the server-controlled `Id` and `CreatedUtc` |
| `DTOs` | `WorkoutRequest` (what clients send, with validation) and `WorkoutResponse` (what the API returns) |
| `Data` | `workouts.json`, created on the first POST. It is git-ignored. |

### Run it

1. Open `FitTrack.Api/FitTrack.Api.slnx` in Visual Studio.
2. Choose the **https** profile and press **F5**. The API listens on `https://localhost:7059`.
3. In development, the OpenAPI document is at `https://localhost:7059/openapi/v1.json`.

### Test it

- **Quick manual checks:** open `FitTrack.Api/FitTrack.Api.http` in Visual Studio and click **Send request** above each block. It covers every endpoint, a 400, a 409, and over-posting.
- **Postman collection:** import `postman/FitTrack.Api.postman_collection.json`. It uses a `{{baseUrl}}` variable (default `https://localhost:7059`) and an id variable set from the POST response. It also covers login and the 401 rules, and is described under Project 4 below. It only deletes records it created itself. In Postman, turn off **SSL certificate verification** (Settings > General) so the local development certificate is accepted.

---

## 🔐 Project 4: Secure API + MVVM MAUI Client

```
MAUI app (MVVM)  --HTTPS + bearer token-->  FitTrack.Api  -->  JSON file   (DataSource = Json)
                                                          -->  SQL Server  (DataSource = SqlServer, in Docker)
```

- **Switchable data source.** One `IWorkoutRepository` interface, two implementations (`JsonWorkoutRepository`, `EfWorkoutRepository`). The `DataSource` setting in `appsettings.json` picks one.
- **SQL Server in Docker.** `FitTrack.Api/compose.yaml`. The password is read from `.env` (git-ignored). The schema is created by an EF Core migration, applied automatically when the API starts.
- **JWT login.** `POST /api/auth/login` returns a token. `POST`, `PUT`, and `DELETE` on workouts need it (policy `WorkoutWrite`). `GET` stays public. Login is limited to 5 attempts per minute per address (then 429).
- **Business rule, enforced twice.** Only one workout per type per day. The service checks it inside a lock (so simultaneous requests cannot both pass), and SQL Server has a unique index on date + type as a backstop.
- **Logging.** Startup lines report the data source and whether authentication is on. Every create, update, and delete is logged with the workout id. Passwords, tokens, and login names are never logged.
- **MAUI client.** Workouts are loaded and saved only through the API. The token lives in `SecureStorage`, and a handler adds `Authorization: Bearer ...` to every request. The app never opens a SQL connection. Meals and goals still use the local SQLite database; only workouts go through the API.

### Where each secret lives (none are in Git)

| Secret | Where it lives |
|---|---|
| SQL Server `sa` password | `FitTrack.Api/.env` (git-ignored) and the connection string in User Secrets |
| JWT signing key | User Secrets, created by `--setup-auth` |
| Demo user password | User Secrets, stored only as a PBKDF2 hash, created by `--setup-auth` |
| Postman user name and password | Postman collection variables, *current value* only (never exported) |

### From a fresh clone to a running app

You need: the .NET 10 SDK, Docker Desktop (running), the EF tool (`dotnet tool install --global dotnet-ef`), and Visual Studio with the .NET MAUI workload plus an Android emulator.

Run every command from the repo root unless it says otherwise.

**1. Clone and trust the dev certificate**

```
git clone https://github.com/DylanMorzfeld/Intro-to-Mobile-Dev.git
cd Intro-to-Mobile-Dev
dotnet dev-certs https --trust
```

**2. Start SQL Server**

```
cd FitTrack.Api
copy .env.example .env
```

Open `.env` and set `MSSQL_SA_PASSWORD` to a strong password (8+ characters with upper case, lower case, a digit, and a symbol). Then:

```
docker compose up -d
docker compose ps
```

Wait until `docker compose ps` shows the container as **healthy** (about 30 to 60 seconds). Then go back: `cd ..`

**3. Give the API its secrets (User Secrets, not files in the repo)**

The connection string, using the same password you put in `.env`:

```
dotnet user-secrets set "ConnectionStrings:FitTrack" "Server=localhost,1433;Database=FitTrack;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True" --project FitTrack.Api
```

The signing key and the demo user (it asks for a user name and a password of 8+ characters):

```
dotnet run --project FitTrack.Api --launch-profile https -- --setup-auth
```

**4. Choose the data source and turn authentication on**

- `FitTrack.Api/appsettings.json`: set `"DataSource": "SqlServer"` (or `"Json"` to use the file in `FitTrack.Api/Data/`).
- `FitTrack.Api/appsettings.Development.json`: set `"Authentication": { "Enabled": true }` (or `false` for open writes).

The migration is already in `FitTrack.Api/Migrations`, so there is nothing to create on a fresh clone. (If you change the model: `dotnet ef migrations add <Name> --project FitTrack.Api`.)

**5. Run the API**

```
dotnet run --project FitTrack.Api --launch-profile https
```

You should see these lines near the top of the log:

```
info: FitTrack.Api[0]
      Database migrations applied          (SqlServer only)
info: FitTrack.Api[0]
      Data source: SqlServer
info: FitTrack.Api[0]
      Authentication: ON (POST, PUT, and DELETE require a valid token)
```

Every create, update, and delete then logs a line such as `Created workout 4: Running on 2026-10-25`.

**6. Run the MAUI app**

Leave the API running. In Visual Studio open `FitTrack/FitTrack/FitTrack.slnx`, pick an Android emulator, and press F5. On Android the app reaches the API at `https://10.0.2.2:7059/` (the emulator's name for your PC). That debug-only address and certificate setting live in `Services/Api/ApiSettings.cs` and `DevCertificateHandler.cs`.

- Browsing the Workouts tab needs no login.
- Add, edit, delete, and complete all need a token. Without one you are sent to the login page. Use the user name and password from step 3.
- Errors shown in the app: field errors from the API (for example a duration of 2000 gives "Duration must be between 1 and 1440 minutes."), a duplicate type on the same day, a wrong password, "Can't reach the API. Is it running?" when the API is off, and "Please log in to make changes." when the token is missing or rejected.

### Postman: proving the auth rules

Import `postman/FitTrack.Api.postman_collection.json`. In the collection's **Variables** tab, type the demo user name and password into the **Current value** column of `username` and `password` (do not use Initial value, which is what gets exported). In Settings, turn off SSL certificate verification so the development certificate is accepted.

Run the whole collection in the Runner, in order. The first request logs in and detects whether authentication is on, so the same collection is green both ways:

| API setting | Result |
|---|---|
| Authentication off | All green. The 401 checks are skipped (they only make sense with auth on). |
| Authentication on | All green. Writes without a token, with a garbage token, and deletes without a token each return 401. Reads without a token return 200. |

The tests only create and delete records whose notes start with `postman-test:` on a random far-future date, so they never touch real data. The login limit is 5 attempts a minute and each run uses 2, so avoid running it more than twice in one minute (or restart the API).

### Troubleshooting

- **Container never becomes healthy:** the `.env` password is too weak, or `docker compose logs sqlserver` says so. Fix `.env`, then `docker compose down -v` and `docker compose up -d`.
- **API says "ConnectionStrings:FitTrack is not set":** run step 3 again.
- **API says authentication is enabled but the key or user is missing:** run `--setup-auth`.
- **App shows "Can't reach the API":** the API is not running, or you started it with the `http` profile instead of `https`.
- **Login returns 429:** wait a minute (or restart the API).
- **Everything is 401 after restarting the API:** log in again. The token is signed with the key in User Secrets; if you re-ran `--setup-auth` on a different machine the key differs.
