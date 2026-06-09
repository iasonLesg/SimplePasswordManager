# 🔒 Simple Password Manager

An open-source, local-first password manager built with modern C# and WPF. 

In a world of cloud data breaches, **Simple Password Manager** takes a different approach: your passwords never leave your computer. Everything is encrypted and stored locally, giving you complete control over your own data.

## ✨ Features
* **100% Local Storage:** No cloud syncing, no external servers. Your vault lives strictly on your hard drive.
* **Modern WPF UI:** Clean, dark-themed, and responsive interface.
* **Built-in Password Generator:** Quickly generate strong, secure passwords on the fly.
* **Import/Export:** Easily backup your vault or merge data using `.vault` files.
* **Open Source:** Completely transparent codebase so you know exactly how your data is being handled.

## 🚀 Getting Started (Highly Recommended Security Step)

While you can run the application directly, the safest way to use an open-source password manager is to compile it yourself with your own unique encryption keys. 

**For the best security results, please follow these steps:**

1. **Clone or Download** this repository to your local machine.
2. Open the project in Visual Studio Community.
3. **Change the Salt Value:** Locate the encryption helper class in the source code and modify the default "salt" string to a completely random, custom phrase of your own. This ensures your `.vault` files are uniquely encrypted to *your* specific build.
4. Build the project in **Release** mode.
5. Create a shortcut to the `.exe` and enjoy!

## 🛠️ Built With
* C# (.NET)
* Windows Presentation Foundation (WPF)

## 🤝 Contributing
Contributions, issues, and feature requests are welcome! Feel free to fork the repo and submit a pull request.

## 📄 License
This project is open-source and available to use, modify, and distribute. 

---
*Built with privacy in mind. Enjoy!*
