import 'package:flutter/material.dart';
import 'package:flutter_unity/flutter_unity.dart';

void main() => runApp(App());

class App extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      home: HomePage(),
    );
  }
}

class HomePage extends StatefulWidget {
  @override
  _HomePageState createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('3D Model Prototype'),
      ),
      body: Center(
        child: RaisedButton(
          onPressed: () {
            Navigator.push(context, MaterialPageRoute(builder: (context) => UnityViewPage()));
          },
          child: Text('Start Prototype'),
        ),
      ),
    );
  }
}

class Vehicle {
  static final String model3 = 'model3';
  static final String models = 'models';
  static final String modelx = 'modelx';
  static final String modely = 'modely';
}

class UnityViewPage extends StatefulWidget {
  @override
  _UnityViewPageState createState() => _UnityViewPageState();
}

class _UnityViewPageState extends State<UnityViewPage> {
  PageController pageController = PageController();

  UnityViewController unityViewController;
  double _sliderValue = 0.0;
  double _speed = 0.0;
  bool isDarkTheme = true;
  String selectedModel = 'Prototype';
  bool isCharging = false;

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
      appBar: AppBar(
        title: Text(selectedModel?.toUpperCase() ?? ''),
        actions: [
          IconButton(
            icon: const Icon(Icons.color_lens_outlined),
            onPressed: () {
              if (isDarkTheme) {
                unityViewController.send(
                  'FrontCamera',
                  'SetBackgroundColor',
                  '245.0,245.0,245.0',
                );
                setState(() {
                  isDarkTheme = false;
                });
              } else {
                unityViewController.send(
                  'FrontCamera',
                  'SetBackgroundColor',
                  '33.0,33.0,33.0',
                );
                setState(() {
                  isDarkTheme = true;
                });
              }
            },
          ),
        ],
      ),
      body: PageView(
        pageSnapping: true,
        onPageChanged: (i) {
          switch (i) {
            case 0:
              unityViewController.send('Loader', 'loadScene', 'model3');
              break;
            case 1:
              unityViewController.send('Loader', 'loadScene', 'models');
              break;
            case 2:
              unityViewController.send('Loader', 'loadScene', 'modelx');
              break;
            case 3:
              unityViewController.send('Loader', 'loadScene', 'modely');
              break;
            default:
          }
        },
        controller: pageController,
        children: [
          /// model 3
          Card(
            color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(20),
            ),
            elevation: 10,
            child: SingleChildScrollView(
              physics: BouncingScrollPhysics(),
              child: Column(
                children: [
                  Card(
                    child: SwitchListTile.adaptive(
                      title: Text('isCharging'),
                      secondary: isCharging
                          ? Icon(
                              Icons.battery_charging_full,
                              color: Colors.green.shade500,
                            )
                          : Icon(Icons.battery_full),
                      value: isCharging,
                      onChanged: (val) {
                        setState(() {
                          isCharging = val;
                        });
                        unityViewController.send(
                          'FrontCamera',
                          'IsCharging',
                          '$val',
                        );
                      },
                    ),
                  ),
                  AspectRatio(
                    aspectRatio: 1.66,
                    child: UnityView(
                      onCreated: onUnityViewCreated,
                      onReattached: onUnityViewReattached,
                      onMessage: onUnityViewMessage,
                    ),
                  ),
                  Card(
                    child: Column(
                      children: [
                        const Text(
                          'Car Speed:',
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(
                          height: 10,
                        ),
                        Slider.adaptive(
                          value: _speed,
                          min: 0,
                          max: 10,
                          divisions: 10,
                          label: _speed.toString(),
                          onChanged: (val) {
                            setState(() {
                              _speed = val;
                            });
                            unityViewController.send(
                              'model3',
                              'SetCarSpeed',
                              '${val.toInt()}',
                            );
                          },
                        ),
                      ],
                    ),
                  ),
                  Card(
                    color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
                    child: Wrap(
                      children: [
                        RaisedButton(
                          child: Text('Fahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'driverfrontdoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Fahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'driverfrontdoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Fahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'driverreardoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Fahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'driverreardoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Beifahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'passengerfrontdoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Beifahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'passengerfrontdoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Beifahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'passengerreardoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Beifahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'passengerreardoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Trunk öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'trunk:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Trunk schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'trunk:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Frunk öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'frunk:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Frunk schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'model3',
                              'DoorOpened',
                              'frunk:false',
                            );
                          },
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),

          /// model s
          Card(
            color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(20),
            ),
            elevation: 10,
            child: SingleChildScrollView(
              physics: BouncingScrollPhysics(),
              child: Column(
                children: [
                  Card(
                    child: SwitchListTile.adaptive(
                      title: Text('isCharging'),
                      secondary: isCharging
                          ? Icon(
                              Icons.battery_charging_full,
                              color: Colors.green.shade500,
                            )
                          : Icon(Icons.battery_full),
                      value: isCharging,
                      onChanged: (val) {
                        setState(() {
                          isCharging = val;
                        });
                        unityViewController.send(
                          'FrontCamera',
                          'IsCharging',
                          '$val',
                        );
                      },
                    ),
                  ),
                  AspectRatio(
                    aspectRatio: 1.66,
                    child: UnityView(
                      onCreated: onUnityViewCreated,
                      onReattached: onUnityViewReattached,
                      onMessage: onUnityViewMessage,
                    ),
                  ),
                  Card(
                    child: Column(
                      children: [
                        const Text(
                          'Car Speed:',
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(
                          height: 10,
                        ),
                        Slider.adaptive(
                          value: _speed,
                          min: 0,
                          max: 10,
                          divisions: 10,
                          label: _speed.toString(),
                          onChanged: (val) {
                            setState(() {
                              _speed = val;
                            });
                            unityViewController.send(
                              'models',
                              'SetCarSpeed',
                              '${val.toInt()}',
                            );
                          },
                        ),
                      ],
                    ),
                  ),
                  Card(
                    color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
                    child: Wrap(
                      children: [
                        RaisedButton(
                          child: Text('Fahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'driverfrontdoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Fahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'driverfrontdoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Fahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'driverreardoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Fahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'driverreardoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Beifahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'passengerfrontdoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Beifahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'passengerfrontdoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Beifahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'passengerreardoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Beifahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'passengerreardoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Trunk öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'trunk:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Trunk schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'trunk:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Frunk öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'frunk:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Frunk schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'models',
                              'DoorOpened',
                              'frunk:false',
                            );
                          },
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),

          /// model x
          Card(
            color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(20),
            ),
            elevation: 10,
            child: SingleChildScrollView(
              physics: BouncingScrollPhysics(),
              child: Column(
                children: [
                  Card(
                    child: SwitchListTile.adaptive(
                      title: Text('isCharging'),
                      secondary: isCharging
                          ? Icon(
                              Icons.battery_charging_full,
                              color: Colors.green.shade500,
                            )
                          : Icon(Icons.battery_full),
                      value: isCharging,
                      onChanged: (val) {
                        setState(() {
                          isCharging = val;
                        });
                        unityViewController.send(
                          'FrontCamera',
                          'IsCharging',
                          '$val',
                        );
                      },
                    ),
                  ),
                  AspectRatio(
                    aspectRatio: 1.66,
                    child: UnityView(
                      onCreated: onUnityViewCreated,
                      onReattached: onUnityViewReattached,
                      onMessage: onUnityViewMessage,
                    ),
                  ),
                  Card(
                    child: Column(
                      children: [
                        const Text(
                          'Car Speed:',
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(
                          height: 10,
                        ),
                        Slider.adaptive(
                          value: _speed,
                          min: 0,
                          max: 10,
                          divisions: 10,
                          label: _speed.toString(),
                          onChanged: (val) {
                            setState(() {
                              _speed = val;
                            });
                            unityViewController.send(
                              'modelx',
                              'SetCarSpeed',
                              '${val.toInt()}',
                            );
                          },
                        ),
                      ],
                    ),
                  ),
                  Card(
                    color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
                    child: Wrap(
                      children: [
                        RaisedButton(
                          child: Text('Fahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'driverfrontdoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Fahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'driverfrontdoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Fahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'driverreardoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Fahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'driverreardoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Beifahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'passengerfrontdoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Beifahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'passengerfrontdoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Beifahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'passengerreardoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Beifahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'passengerreardoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Trunk öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'trunk:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Trunk schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'trunk:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Frunk öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'frunk:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Frunk schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modelx',
                              'DoorOpened',
                              'frunk:false',
                            );
                          },
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),

          /// model y
          Card(
            color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(20),
            ),
            elevation: 10,
            child: SingleChildScrollView(
              physics: BouncingScrollPhysics(),
              child: Column(
                children: [
                  Card(
                    child: SwitchListTile.adaptive(
                      title: Text('isCharging'),
                      secondary: isCharging
                          ? Icon(
                              Icons.battery_charging_full,
                              color: Colors.green.shade500,
                            )
                          : Icon(Icons.battery_full),
                      value: isCharging,
                      onChanged: (val) {
                        setState(() {
                          isCharging = val;
                        });
                        unityViewController.send(
                          'FrontCamera',
                          'IsCharging',
                          '$val',
                        );
                      },
                    ),
                  ),
                  AspectRatio(
                    aspectRatio: 1.66,
                    child: UnityView(
                      onCreated: onUnityViewCreated,
                      onReattached: onUnityViewReattached,
                      onMessage: onUnityViewMessage,
                    ),
                  ),
                  Card(
                    child: Column(
                      children: [
                        const Text(
                          'Car Speed:',
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(
                          height: 10,
                        ),
                        Slider.adaptive(
                          value: _speed,
                          min: 0,
                          max: 10,
                          divisions: 10,
                          label: _speed.toString(),
                          onChanged: (val) {
                            setState(() {
                              _speed = val;
                            });
                            unityViewController.send(
                              'modely',
                              'SetCarSpeed',
                              '${val.toInt()}',
                            );
                          },
                        ),
                      ],
                    ),
                  ),
                  Card(
                    color: isDarkTheme ? Color.fromRGBO(33, 33, 33, 1) : Color.fromRGBO(245, 245, 245, 1),
                    child: Wrap(
                      children: [
                        RaisedButton(
                          child: Text('Fahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'driverfrontdoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Fahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'driverfrontdoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Fahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'driverreardoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Fahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'driverreardoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Beifahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'passengerfrontdoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Beifahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'passengerfrontdoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Beifahrertür öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'passengerreardoor:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Hintere Beifahrertür schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'passengerreardoor:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Trunk öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'trunk:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Trunk schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'trunk:false',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Frunk öffnen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'frunk:true',
                            );
                          },
                        ),
                        RaisedButton(
                          child: Text('Frunk schließen'),
                          onPressed: () {
                            unityViewController.send(
                              'modely',
                              'DoorOpened',
                              'frunk:false',
                            );
                          },
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  void onUnityViewCreated(UnityViewController controller) {
    debugPrint('onUnityViewCreated');

    setState(() {
      unityViewController = controller;
      controller.send('Loader', 'loadScene', 'model3');
    });
  }

  void onUnityViewReattached(UnityViewController controller) {
    debugPrint('onUnityViewReattached');
    setState(() {
      unityViewController = controller;
    });
  }

  void onUnityViewMessage(UnityViewController controller, String message) {
    debugPrint('onUnityViewMessage');
  }
}
