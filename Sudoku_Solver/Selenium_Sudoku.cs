/*
    Ethan Eill      03/16/2022

    Sudoku Solver using selenium to automatically travel to website, grab 
    puzzle, solve, then output to website without user input
*/

using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

namespace Selenium_Sudoku{

    class Program{

        //2D array of sudoku values
        private static char[,] grid = new char[9,9];

        //used for setting user specified difficulty
        static string setDifficulty(){
            string difficulty = "";
            int input = 0;
            //get input and store in int input
            while(input < 1 || input > 4){
                Console.WriteLine("What difficulty would you like to play?");
                Console.WriteLine("1.Easy\n2.Medium\n3.Hard");
                input = Convert.ToInt32(Console.ReadLine());
                if(input < 1 || input > 3) Console.WriteLine("Please enter valid input");
            }
            //travel to specified difficulty
            if(input == 1) difficulty = "https://www.nytimes.com/puzzles/sudoku/easy";
            else if(input == 2) difficulty = "https://www.nytimes.com/puzzles/sudoku/medium";
            else if(input == 3) difficulty = "https://www.nytimes.com/puzzles/sudoku/hard";

            return difficulty;
        }

        static IWebElement puzzleWait(IWebDriver driver){
            IWebElement puzzleFrame = driver.FindElement(By.Id("js-hook-game-wrapper"));
            return puzzleFrame;
        }

        static void initializeGrid(IWebDriver driver){
            //wait until puzzle has loaded in
            WebDriverWait waitPuzzle = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            waitPuzzle.Until(puzzleWait);
            
            //loop through grid
            for(int i = 0; i < 9; i++){
                for(int j = 0; j < 9; j++){
                    //for grabbing corresponding div for grid[i,j]
                    string divIndex = "";
                    divIndex += ((i*9)+(j+1));
                    IWebElement cell = driver.FindElement
                    (By.XPath("//*[@id='pz-game-root']/div[2]/div/div[1]/div/div/div/div["+divIndex+"]"));
                    //store cell num in string value
                    string value = cell.GetAttribute("ariaLabel");
                    //store value in 2D char array 
                    if(value == "empty") grid[i,j] = '0';
                    else grid[i,j] = value.ToCharArray()[0];
                }
            }
        }

        //check to make sure desired move does not violate any sudoku rules
        static bool isMove(int y, int x, char c){

            for (int i = 0; i < 9; i++){  
                //check row  
                if (grid[i, x] != '0' && grid[i, x] == c) return false;

                //check column  
                if (grid[y, i] != '0' && grid[y, i] == c) return false;

                //check 3*3 block  
                if (grid[3 * (y / 3) + i / 3, 3 * (x / 3) + i % 3] != '0' &&
                 grid[3 * (y / 3) + i / 3, 3 * (x / 3) + i % 3] == c) return false;

            } 
            return true;
        }

        //check to make sure grid is initialized properly
        static void solveSudoku(){  
            if (grid == null || grid.Length == 0) return;  
            solve();  
        }  

        //main solver
        static bool solve(){

            //loop through grud
            for (int i = 0; i < grid.GetLength(0); i++){  
                for (int j = 0; j < grid.GetLength(1); j++){ 
                    //if cell is empty 
                    if (grid[i, j] == '0'){
                        //loop through all possible characters
                        for (char c = '1'; c <= '9'; c++){
                            if (isMove(i, j, c)){
                                //if it is valid move then set grid[i,j] = c
                                grid[i, j] = c;

                                //recursive call
                                //otherwise move is not accurate, backtrack
                                if (solve()) return true;
                                else grid[i, j] = '0';
                            }
                        }  
                        return false;  
                    }  
                }  
            }  
            return true;  
        }

        //outputs solved puzzle to website
        static void outToWeb(IWebDriver driver){
            //to make sure not using array keys without pressing values
            int numPressed = 0;
            //loop through 2D array
            for(int i = 0; i < 9; i++){
                for(int j = 0; j < 9; j++){
                    //divIndex for counting number of divs to traverse through in html
                    string divIndex = "";
                    divIndex += ((i*9)+(j+1));
                    //find corresponding cell with 2D array
                    IWebElement cell = driver.FindElement
                    (By.XPath("//*[@id='pz-game-root']/div[2]/div/div[1]/div/div/div/div["+divIndex+"]"));
                    string value = cell.GetAttribute("ariaLabel");
                    //if cell is empty sendkeys
                    if(value == "empty"){
                        new Actions(driver).SendKeys(grid[i,j].ToString()).Perform();
                        new Actions(driver).SendKeys(Keys.ArrowRight).Perform();
                        numPressed++;
                    //else if we are in correct position then go right
                    }else if(numPressed!=0){
                        new Actions(driver).SendKeys(Keys.ArrowRight).Perform();
                    }
                }
                //at end of curr row go down and all the way to the left
                new Actions(driver).SendKeys(Keys.ArrowDown).Perform();
                for(int x = 0; x < 8; x++){
                    new Actions(driver).SendKeys(Keys.ArrowLeft).Perform();
                }
            }
        }
        
        static void Main(string[] args){
            string url = setDifficulty();
            //set up website to travel to
            IWebDriver driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(url);

            //scroll to puzzle in view
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollTo(0, 500)");

            //initialize 2D array of values given initially
            initializeGrid(driver);
            //solve 2D array
            solveSudoku();

            //output solution to web
            outToWeb(driver);

            Thread.Sleep(5000);

            driver.Quit();

        }
    }
}
