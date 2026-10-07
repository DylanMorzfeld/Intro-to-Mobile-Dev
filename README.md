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
