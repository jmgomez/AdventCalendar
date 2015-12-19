namespace AdventCalendar

open System

open UIKit
open Foundation

open Xamarin.Forms

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit UIApplicationDelegate ()

    override val Window = null with get, set

    // This method is invoked when the application is ready to run.
    override this.FinishedLaunching (app, options) =
       Xamarin.Forms.Forms.Init();
       this.Window <- new UIWindow(UIScreen.MainScreen.Bounds) 
       let movieDBApp = View.TheMovieDB()
       movieDBApp.Start()
       this.Window.RootViewController <- movieDBApp.MainPage.CreateViewController()
       this.Window.MakeKeyAndVisible()
       true