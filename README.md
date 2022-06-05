﻿<div id="top"></div>


<!-- PROJECT LOGO -->
<br />
<div align="center">

<h3 align="center">FACEIT Discord Rich Presence</h3>

  <p align="center">
    A C# application which adds Discord RichPresence with FACEIT data.
    <br />
    <br />
    <br />
    <a href="https://github.com/DamsDev1/FACEIT-Discord-RichPresence/issues/new?assignees=&labels=&template=bug_report.md&title=">Report Bug</a>
    ·
    <a href="https://github.com/DamsDev1/FACEIT-Discord-RichPresence/issues/new?assignees=&labels=&template=feature_request.md&title=">Request Feature</a>
  </p>
</div>


<!-- ABOUT THE PROJECT -->
## About The Project
![Product Name Screen Shot][product-screenshot]

This application collects information about your current game on FACEIT.

It shows it to all your friends on Discord with RichPresence.

They can know the score, the map and the game time.

### Built With

* [C#](https://visualstudio.microsoft.com/)

<!-- GETTING STARTED -->
## Getting Started

### Installation

1. Get a free API Key at [https://developers.faceit.com/](https://developers.faceit.com/)
2. Clone the repo
   ```sh
   git clone https://github.com/DamsDev1/FACEIT-RichPresence-Discord.git
   ```
3. Install NPM packages
   ```sh
   npm install
   ```
4. Enter your FACEIT API in `config.js`
   ```js
   const FaceitAPIKey = 'ENTER YOUR API';
   ```
5. Enter your FACEIT Player ID `config.js`
   ```js
   const FaceitAPIKey = 'ENTER PLAYER ID';
   ```
6. Run the application with
   ```sh
   node index.js
   ```

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[product-screenshot]: images/screenshot.png