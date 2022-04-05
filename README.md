# Colour Picker
## Our LudumDare 50 game


<img width="1800" alt="Screenshot 2022-04-05 at 23 03 00" src="https://user-images.githubusercontent.com/3743913/161861312-f696d1dc-9468-41d2-9a27-4c3cb4fbe3fe.png">

<img width="1912" alt="image" src="https://user-images.githubusercontent.com/3743913/161861380-c4d82932-154f-42de-9f90-f3170a2a0be4.png">


Heya. For this year, my wife and I decided to try something completely out of our comfort zone.
Instead of making a game in a traditional game engine, we decided to make a turn based multiplayer game using Blazor Server.

As a result of that we didn't have much time for balancing and tweaking the gameplay (not to mention the occasional syncing bug). That being said, it was a lot of fun to try something very different for a change, and I will definetely snazzy up this game properly to play with friends and family :)

To run this game locally, you will have to insall dotnet core version 6+

To run it, open up the project in the terminal of your choice and write:
```dotnet restore```, followed by ```dotnet watch```

The game is a blazor server side application.

For your convenience I've also setup a hosted solution here:

https://colour-picker.azurewebsites.net/

However keep in mind that its is a free hosting solution and as such the performance reflects that. Additionally it will only be available for 60 mins a day.

## Tutorial
<img width="575" alt="image" src="https://user-images.githubusercontent.com/3743913/161861832-a283ef62-fb10-4dc4-9aca-c47d4eaf9370.png">

To play the game, you need to either host a game or join one.


### Hosting
<img width="1389" alt="image" src="https://user-images.githubusercontent.com/3743913/161861931-01f2f297-99df-47cc-84f0-e7a776bf2b69.png">

After choosing a name and an avatar, you are presented with the game session. From here you may copy the game code in the top left corner and send it to whomever you want joining your game.

### Joining
<img width="627" alt="image" src="https://user-images.githubusercontent.com/3743913/161861988-9ca1f3ed-bd81-4395-beff-b22b7697b42b.png">

You will have to wait for a friend to send you a game code. Once you've entered it, you have to select and avatar.
Upon doing so you will have joined the game.

### playing the game
<img width="1912" alt="Screenshot 2022-04-05 at 23 35 29" src="https://user-images.githubusercontent.com/3743913/161862255-7f95a5c8-88cc-4f28-93d1-d4396d335163.png">

Once there are more than 1 player in the game session, you are able to press the "start" button.
When the game starts, the timer also starts. One of you is given the turn (indicating by your avatar growing in size) - during your turn you have to attempt to select a colour on the wheel that matches the background to the best of your abilities.

<img width="1912" alt="Screenshot 2022-04-05 at 23 35 36" src="https://user-images.githubusercontent.com/3743913/161862722-e23ee704-8dba-4627-bbf7-900e4c86ead1.png">

Upon doing so will add a a bit of additional time to the countdown. The amount will vary depending on how accurate your guess was. If it was below 60% accuracy, it will not add any additional time. Your turn ends after selecting a colour - giving the turn to the next player.
This keeps proceeding as such until the timer reaches 0. The one who's turn it was when the timer ends is terminated from the game.

The game ends when there is only a single player left.


## Bugs
Due to lack of time these were the missing bugs and/or features that were never considered:
- Announce the winner after the game ends
- Don't allow more than 10 players
- Skip the dead player's turn
- Visually show what colour the other player selected
- Increase the speed at which the timer goes down for every turn
- End the visual frontend timer when the game ends

- Fix occasional syncing issues when joining a game

