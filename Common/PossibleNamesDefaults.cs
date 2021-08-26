﻿using Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using System.Threading.Tasks;

namespace Backend
{
    public static class PossibleNamesDefaults
    {
        public  static readonly string[] FirstNames = new string[]
        {
            "Always",
"Amazing",
"Angry",
"Awesome",
"Balanced",
"Best",
"Big",
"Bitter",
"Bold",
"Bonkers",
"Bouncy",
"Brave",
"Bright",
"Bubbly",
"Bumbly",
"Chaotic",
"Chatty",
"Cheeky",
"Cheery",
"Chic",
"Chill",
"Chilled",
"Chilly",
"Chrome",
"Classic",
"Clever",
"Cloudy",
"Clumsy",
"Cold",
"Colossal",
"Cool",
"Crafty",
"Crazy",
"Cuddly",
"Dizzy",
"Dramatic",
"Dreamy",
"Electric",
"Epic",
"Fabulous",
"Famous",
"Fancy",
"Fast",
"Feisty",
"Firey",
"First",
"Fluffy",
"Freezing",
"Friendly",
"Fun",
"Funky",
"Gentle",
"Giddy",
"Gifted",
"Glorious",
"Goofy",
"Googly",
"Graceful",
"Great",
"Grey",
"Groovy",
"Gutsy",
"Happy",
"Hasty",
"Hazy",
"Heroic",
"Hyper",
"Infallible",
"Infamous",
"Janky",
"Jittery",
"Jolly",
"Joyful",
"Jumpy",
"Keen",
"Klutzy",
"Last",
"Lone",
"Loud",
"Lucky",
"Mad",
"Magic",
"Majestic",
"Marvelous",
"Mega",
"Merry",
"Mighty",
"Mystic",
"Nerdy",
"Nifty",
"Nimble",
"Nippy",
"Noble",
"Orange",
"Perfect",
"Playful",
"Popular",
"Posh",
"Powerful",
"Prickly",
"Prized",
"Quick",
"Quiet",
"Rad",
"Rapid",
"Rare",
"Real",
"Red",
"Rocky",
"Rolly",
"Round",
"Royal",
"Scared",
"Scary",
"Second",
"Secret",
"Shady",
"Shiny",
"Silly",
"Sleepy",
"Slick",
"Slimy",
"Small",
"Smart",
"Smashing",
"Smooth",
"Snappy",
"Sneaky",
"Sparkly",
"Special",
"Spicy",
"Spiky",
"Steady",
"Stout",
"Strong",
"Stumbly",
"Sunny",
"Super",
"Superb",
"Sweet",
"Swift",
"Talented",
"Third",
"Tiny",
"Toothy",
"Tough",
"Tricky",
"Tumbling",
"Tumbly",
"Unbeaten",
"Unlucky",
"Unsteady",
"Upbeat",
"Valiant",
"Vivid",
"Wacky",
"Weird",
"Wild",
"Wise",
"Wobbly",
"Wonky",
"Zany"
        };

        public static readonly string[] SecondNames = new string[]
        {
            "Adoring",
"Beaming",
"Bitter",
"Blue",
"Boss",
"Bouncing",
"Bronze",
"Bubbling",
"Bumbling",
"Charging",
"Chasing",
"Chilling",
"Chuffed",
"Confused",
"Confusing",
"Crying",
"Crystal",
"Cyan",
"Dancing",
"Daring",
"Dark",
"Dazzling",
"Diamond",
"Diving",
"Dodging",
"Dreaming",
"Evil",
"Fall",
"Fallen",
"Falling",
"Fizzing",
"Fleeing",
"Flying",
"Frantic",
"Gaming",
"Giggling",
"Gleaming",
"Glowing",
"Golden",
"Good",
"Googly",
"Green",
"Honking",
"Hopping",
"Jeweled",
"Jumping",
"Laughing",
"Leaping",
"Light",
"Lilac",
"Little",
"Major",
"Master",
"Mini",
"Pink",
"Playing",
"Popping",
"Puzzling",
"Raring",
"Red",
"Relaxing",
"Rocking",
"Rolling",
"Ruby",
"Running",
"Scarlet",
"Scheming",
"Secret",
"Silver",
"Singing",
"Skipping",
"Slamping",
"Sledding",
"Sleeping",
"Sliding",
"Smiling",
"Sneaking",
"Snoozy",
"Snoring",
"Sparkling",
"Sprinting",
"Stamping",
"Stomping",
"Streaming",
"Stumbling",
"Swimming",
"Talking",
"Teal",
"Thinking",
"Tiptoeing",
"Toppling",
"Travelling",
"Tumbling",
"Vibrant",
"Winning",
"Yeeted",
"Yellow",
"Zooming"
        };

        public static readonly string[] ThirdNames = new string[]
        {
            "Artist",
            "Acrobat",
"Android",
"Animal",
"Apple",
"Athlete",
"Autumn",
"Baller",
"Baron",
"Bean",
"Bear",
"Bestie",
"Bike",
"Bird",
"Biscuit",
"Blob",
"Bolt",
"Bonkus",
"Boss",
"Boxer",
"Buddy",
"Bunny",
"Burger",
"Butler",
"Butterfly",
"Cactus",
"Cat",
"Celeb",
"Challenger",
"Champ",
"Champion",
"Charger",
"Charmer",
"Chaser",
"Cheese",
"Cheetah",
"Chum",
"Climber",
"Clown",
"Coffee",
"Comet",
"Cookie",
"Creature",
"Crown",
"Cyborg",
"Dancer",
"Dandy",
"Dash",
"Day",
"Dino",
"Disco",
"Diver",
"Dog",
"Domino",
"Dove",
"Dragon",
"Dream",
"Duck",
"Dude",
"Duke",
"Eagle",
"Elf",
"Emperor",
"Empress",
"Feast",
"Fish",
"Fizz",
"Flash",
"Flor",
"Flower",
"Flyer",
"Friend",
"Fries",
"Frog",
"Gal",
"Gamer",
"Ghost",
"Giggler",
"Goliath",
"Goof",
"Goose",
"Grabber",
"Grape",
"Griffin",
"Grizzly",
"Gubbins",
"Guy",
"Hammer",
"Hamster",
"Hare",
"Hero",
"Hill",
"Hippo",
"Horse",
"Hotdog",
"Hugger",
"IceCream",
"Jelly",
"Joker",
"Jumper",
"Junior",
"King",
"Kitten",
"Knight",
"Koala",
"Lad",
"Lass",
"Lemon",
"Lion",
"Master",
"Melon",
"Meme",
"Mermaid",
"Monster",
"Moon",
"Morning",
"Mouse",
"Muffin",
"MVP",
"Night",
"Ninja",
"Ogre",
"Oliver",
"Orange",
"Owl",
"Panther",
"Penaut",
"Penguin",
"Pigeon",
"Pirate",
"Planet",
"Prince",
"Princess",
"Pudding",
"Punk",
"Puppy",
"Queen",
"Racer",
"Raptor",
"Rebel",
"Robot",
"Roller",
"Rookie",
"Royalty",
"Runner",
"Samurai",
"Senior",
"Shark",
"Sheep",
"Sidekick",
"Skelly",
"Skills",
"Sloth",
"Snake",
"Snowflake",
"Sparrow",
"Spinner",
"Spirit",
"Sponge",
"Spring",
"Star",
"Streamer",
"Student",
"Summer",
"Sun",
"Superhero",
"Surfer",
"Sweeper",
"Tail",
"Tea",
"Tiger",
"Tomato",
"Toon",
"Treasure",
"Tree",
"Turtle",
"Unicorn",
"Vampire",
"Victor",
"Viking",
"Villain",
"VIP",
"Virtuoso",
"Wallflower",
"Werewolf",
"Winner",
"Winter",
"Witch",
"Wizard",
"Wolf",
"Yeeter",
"Yeetus",
"Yeti",
"Zebra",
"Zoomer"        };
    }
}