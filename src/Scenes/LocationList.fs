module internal LocationList

open System
open Fable.Core
open Fable.Import
open Fable.Import.ReactNative
open Fable.Import.ReactNativeImagePicker
open Fable.Helpers.ReactNative
open Fable.Helpers.ReactNative.Props
open Fable.Core.JsInterop
open Elmish
open Messages

// Model
type Status =
| NotStarted
| InProgress
| Complete of string

type Model =
  { RequestDataSource : ListViewDataSource<int * Model.LocationCheckRequest>
    Status : Status } 

let init () =
    { Status = NotStarted
      RequestDataSource = emptyDataSource() }, Cmd.ofMsg LocationListMsg.RefreshList |> Cmd.map LocationListMsg

// Update
let update msg model : Model*Cmd<AppMsg> =
    match msg with
    | LocationListMsg.CheckNextLocation (pos,request) ->
        { model with
            Status = InProgress }, Cmd.ofMsg (NavigateTo (Page.CheckLocation(pos,request)))
    | LocationListMsg.RefreshList ->
        { model with
            Status = InProgress }, Cmd.ofAsync Database.getIndexedCheckRequests () LocationListMsg.NewLocationCheckRequests LocationListMsg.Error |> Cmd.map LocationListMsg
    | LocationListMsg.NewLocationCheckRequests indexedRequests ->
        { model with
            RequestDataSource = updateDataSource indexedRequests model.RequestDataSource
            Status = Complete (sprintf "Locations: %d" indexedRequests.Length) }, []
    | LocationListMsg.Error e ->
        { model with
            Status = Complete (string e.Message) }, []

// View
let view (model:Model) (dispatch: AppMsg -> unit) =
    let getListView() =
          listView 
            model.RequestDataSource
            [ ListViewProperties.RenderRow  
                (Func<_,_,_,_,_>(fun (pos,request) b c d ->
                    view [
                        ViewProperties.Style[
                            ViewStyle.JustifyContent JustifyContent.Center
                            ViewStyle.AlignItems ItemAlignment.Center
                            ViewStyle.Flex 1
                            ViewStyle.FlexDirection FlexDirection.Row ]]
                        [ text [ Styles.defaultText ] request.Name
                          text [ Styles.defaultText ] request.Address
                          (match request.Status with
                           | Model.LocationStatus.NotChecked -> text [] ""
                           |  _ ->
                                let uri = 
                                    match request.Status with
                                    | Model.LocationStatus.Alarm text -> localImage "../../images/Alarm.png"
                                    | _ -> localImage "../../images/Approve.png"

                                image
                                    [ Source uri
                                      ImageProperties.Style [
                                            ImageStyle.Width 24.
                                            ImageStyle.Height 24.
                                            ImageStyle.AlignSelf Alignment.Center
                                        ]
                                    ])
                         ]
                    |> touchableHighlight [OnPress (fun () -> dispatch (LocationListMsg (LocationListMsg.CheckNextLocation(pos,request))))]))
            ]

    view [ Styles.sceneBackground ] 
        [ text [ Styles.titleText ] "Locations to check"
          text [ Styles.defaultText ] 
            (match model.Status with 
             | Complete s -> s
             | _ -> "")
          (if model.RequestDataSource.getRowCount() > 0 then getListView() else Styles.whitespace)

          Styles.button "OK" (fun () -> dispatch AppMsg.NavigateBack)
        ]