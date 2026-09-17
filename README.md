# 💪 FitTrack — Personal Fitness Tracker

A .NET MAUI app for tracking workouts, meals, and fitness goals — built with the MVVM pattern for Intro to Mobile Development.

---

## 📱 What It Does

FitTrack has three main screens:

| Screen | What you can do |
|---|---|
| **Workout Log** | Log workouts (type, duration, intensity, calories). Edit or delete any entry. |
| **Diet Tracker** | Log meals with calories, protein, carbs, and fat. See your daily total vs. your calorie goal. |
| **Goals** | Set a fitness goal (weight loss, muscle gain, endurance) and watch a progress bar fill in as you update it. Hit your goal and get a little celebration message. 🏆 |

---

## 🏗️ How It's Built (MVVM)

The app is split into four clear layers, so each piece only does one job:

- **Models** — plain data (`Workout`, `Meal`, `FitnessGoal`). No app logic, just the data itself.
- **Services** — handles saving/loading data. Currently uses a local SQLite database, but it's built so a real backend API could be swapped in later without changing anything else in the app.
- **ViewModels** — the "brains" of each screen. Holds the data the screen needs and the actions the user can take (like Save or Delete). Has zero knowledge of what the screen actually looks like.
- **Views** — the actual screens (XAML). Just displays data and forwards user taps to the ViewModel — no logic lives here.

**Why split it this way?** If I want to change how data is stored (say, move from local storage to a real server), I only touch the Services layer. Nothing in the ViewModels or Views has to change. Same idea if I want to redesign a screen — I only touch the View.

---

## 🧭 Getting Around the App

Three tabs at the bottom: **Workouts**, **Diet**, **Goals**. Tapping "+" on any tab opens a form to add a new entry; tapping an existing entry opens that same form pre-filled so you can edit it.

---

## 💾 Where the Data Lives

Everything is saved locally on the device using SQLite, so your data sticks around between app launches.

---

## 🔮 What's Next

- Actual reminders for recurring workouts (right now it just remembers you marked one as recurring)
- Real backend/API support instead of local-only storage
- Swipe gestures and other interactive touches (Project 2)

---

## 🛠️ Built With

- .NET MAUI
- CommunityToolkit.Mvvm
- SQLite (sqlite-net-pcl)
