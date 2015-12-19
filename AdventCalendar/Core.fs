

namespace AdventCalendar
open System

open Xamarin.Forms

open FSharp.Data
open System.Collections
open System.Collections.ObjectModel

open System.Linq

module Model =
    type Movie () = 
        member val Title = "" with get, set
        member val ImagePath = "" with get, set 
        member val Overview = "" with get, set 


module TheMovieDB =
    open Model
    let key = "a78afef17280cbb0a141f9918b7f6bda"
    let baseUrl = "http://api.themoviedb.org/3"
    let baseImageUrl = "https://image.tmdb.org/t/p/w185/"
    let [<Literal>] discoverUrl = """{ "page": 1, "results": [{ "adult": false, "backdrop_path": "/dkMD5qlogeRMiEixC4YNPUvax2T.jpg",   "genre_ids": [28,12,878, 53 ],"id": 135397,"original_language": "en", "original_title": "Jurassic World", "overview":"overviewremoved", "release_date": "2015-06-12", "poster_path": "/uXZYawqUsChGSj54wcuBtEdUJbh.jpg","popularity": 88.551849,  "title": "Jurassic World", "video": false,"vote_average": 7.1,  "vote_count": 435 }],"total_pages": 11543, "total_results": 230847}"""
    type MovieList = JsonProvider<discoverUrl>
    let getMoviesByPage page = async {
                    let! response = 
                        String.Format("{0}/discover/movie?api_key={1}&page={2}",baseUrl, key, page)
                        |> MovieList.AsyncLoad
                    
                    return response
                    |> fun js -> js.Results 
                    |> Array.map(fun r -> Movie(Title=r.Title, ImagePath=baseImageUrl+r.PosterPath, Overview=r.Overview))
                }
    

    let getMovies (currentPage, times) = 
            [|currentPage..currentPage+times|] 
            |> Array.map(fun p -> p.ToString())
            |> Array.map(getMoviesByPage)
            |> Async.Parallel
            |> Async.RunSynchronously
            |> Array.collect(fun m -> m)
           
           
          

module View = 
    type MoviePage (movie:Model.Movie) as this =
        inherit ContentPage()
        do 
            let title = Label(Text = movie.Title, FontSize = 18., FontAttributes = FontAttributes.Bold, TextColor = Color.Accent)
            this.Title <- movie.Title

            let img = Image(Source=ImageSource.FromUri(Uri(movie.ImagePath)))
            let overview = Label(Text = movie.Overview)
            let layout = RelativeLayout()
            let m = 10.
            let margin = Constraint.Constant m 
            layout.Children.Add(img, margin, margin)
            layout.Children.Add(title, 
                Constraint.RelativeToView(img, Func<_,_,_>(fun p v -> v.Width + m*2.)),
                Constraint.RelativeToView(img, Func<_,_,_>(fun p v -> v.Height / 2.)),
                Constraint.RelativeToView(img, Func<_,_,_>(fun p v -> p.Width - v.Width - m*2. )))
            layout.Children.Add(overview, 
                margin,         
                Constraint.RelativeToView(img, Func<_,_,_>(fun p v -> v.Height + m*3.)),
                Constraint.RelativeToParent(Func<_,_>(fun p -> p.Width - m*3.)),
                Constraint.RelativeToParent(Func<_,_>(fun p -> p.Width - overview.Y)))

                
            this.Content <- layout

    type MovieCell () = 
        inherit ImageCell()
            do 
               base.SetBinding(TextCell.DetailProperty, "Overview")
               base.SetBinding(TextCell.TextProperty, "Title")
               base.SetBinding(ImageCell.ImageSourceProperty, "ImagePath")



    type MoviesPage () as this =
        inherit ContentPage()
      
        let movies = ObservableCollection<Model.Movie>()
        let mutable currentPage = 1
        let times = 3

        let loadMovies() = 
            if not this.IsBusy then
                Device.BeginInvokeOnMainThread(fun _ ->
                    this.IsBusy <- true
                    TheMovieDB.getMovies (currentPage, times) 
                    |> Array.iter (movies.Add) 
                    currentPage <- currentPage + times
                    this.IsBusy <- false)
             
        do
            this.Title <- "Movies"
            loadMovies()
            let lv = ListView(ItemsSource=movies, SeparatorVisibility=SeparatorVisibility.None, RowHeight=100)
            lv.ItemAppearing.Add(fun(cell)-> if((cell.Item:?>Model.Movie) = movies.Last()) then loadMovies())
            lv.ItemTemplate <- DataTemplate(typeof<MovieCell>)
            lv.ItemSelected.Add(fun(item)->this.Navigation.PushAsync(MoviePage(lv.SelectedItem:?>Model.Movie)) |> ignore)
            base.Content <- lv

        

    type TheMovieDB () =
        inherit Application()

        member x.Start() = 
            base.MainPage <- NavigationPage(MoviesPage())
            ()
