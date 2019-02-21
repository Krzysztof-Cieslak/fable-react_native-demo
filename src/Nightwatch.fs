module Nightwatch

open Elmish
open Elmish.React
open Elmish.ReactNative
// TODO: open Elmish.HMR

let setupBackHandler dispatch =    
    let backHandler () =
        dispatch App.Msg.NavigateBack
        true

    Fable.Helpers.ReactNative.setOnHardwareBackPressHandler backHandler


let subscribe (model:App.Model) =
    Cmd.batch [
        Cmd.ofSub setupBackHandler ]


Program.mkProgram App.init App.update App.view
|> Program.withSubscription subscribe
#if RELEASE
#else
|> Program.withConsoleTrace
#endif
|> Program.withReactNative "nightwatch"
|> Program.run