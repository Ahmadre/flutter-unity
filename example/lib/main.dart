import 'package:flutter/material.dart';
import 'package:flutter_unity/flutter_unity.dart';

void main() => runApp(App());

class App extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
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
        title: const Text('Plugin example app'),
      ),
      body: Center(
        child: RaisedButton(
          onPressed: () {
            Navigator.push(context, MaterialPageRoute(builder: (context) => UnityViewPage()));
          },
          child: Text('Test'),
        ),
      ),
    );
  }
}

class UnityViewPage extends StatefulWidget {
  @override
  _UnityViewPageState createState() => _UnityViewPageState();
}

class _UnityViewPageState extends State<UnityViewPage> {
  UnityViewController unityViewController;
  double _sliderValue;

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
        title: const Text('Plugin example app'),
      ),
      body: Card(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
        ),
        elevation: 10,
        child: Stack(
          children: [
            UnityView(
              onCreated: onUnityViewCreated,
              onReattached: onUnityViewReattached,
              onMessage: onUnityViewMessage,
            ),
            Positioned(
              bottom: 20,
              left: 20,
              right: 20,
              child: Card(
                child: Column(
                  children: [
                    const Text(
                      'Rotation Speed:',
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(
                      height: 10,
                    ),
                    Slider.adaptive(
                      value: _sliderValue,
                      onChanged: (val) {
                        setState(() {
                          _sliderValue = val;
                        });
                        unityViewController.send(
                          'Model3',
                          'SetRotationSpeed',
                          '${val.toInt()}',
                        );
                      },
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void onUnityViewCreated(UnityViewController controller) {
    print('onUnityViewCreated');

    setState(() {
      unityViewController = controller;
    });
  }

  void onUnityViewReattached(UnityViewController controller) {
    print('onUnityViewReattached');
    setState(() {
      unityViewController = controller;
    });
  }

  void onUnityViewMessage(UnityViewController controller, String message) {
    print('onUnityViewMessage');

    print(message);
  }
}
