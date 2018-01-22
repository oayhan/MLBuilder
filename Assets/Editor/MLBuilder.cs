using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MLBuilder
{
    //parameters for building
    private const string ExecutableName = "";   // ml.exe
    private const string SceneName = "";        // mlscene
    private const string ScenePath = "";        // Assets/Scenes/mlscene.unity
    private const string BuildPath = "";        // ../python/

    //parameters for brain
    private const string AcademyName = "Academy";      //change to GameObject name of your academy

    [MenuItem("ML/Build For Training")]
    public static void BuildForTraining()
    {
        Debug.Log("Starting build for external training.");

        Scene trainingScene = GetTrainingScene();
        
        //get academy game object
        GameObject[] gameObjects = trainingScene.GetRootGameObjects();
        GameObject academyObject = gameObjects.FirstOrDefault(g => g.name == AcademyName);
        //if academy is found, get child brain and change brain type
        if (academyObject != null)
        {
            Brain brain = academyObject.GetComponentInChildren<Brain>();
            brain.brainType = BrainType.External;
            brain.UpdateCoreBrains();
        }
        else
        {
            Debug.LogError("Brain not found on current scene!");
        }

        //start build
        string[] scenes = new[] { ScenePath };
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        BuildPipeline.BuildPlayer(scenes, BuildPath + ExecutableName, BuildTarget.StandaloneWindows, BuildOptions.Development);

        Debug.Log("Build completed.");
    }

    [MenuItem("ML/Switch To Internal")]
    public static void SwitchToInternalBrain()
    {
        //get training scene and find academy object
        Scene trainingScene = GetTrainingScene();
        GameObject[] gameObjects = trainingScene.GetRootGameObjects();
        GameObject academyObject = gameObjects.FirstOrDefault(g => g.name == AcademyName);

        if (academyObject != null)
        {
            //get child brain and change brain type
            Brain brain = academyObject.GetComponentInChildren<Brain>();
            brain.brainType = BrainType.Internal;
            brain.UpdateCoreBrains();

            //get selected text asset
            TextAsset selectedTextAsset = (TextAsset)Selection.activeObject;
            if (selectedTextAsset != null)
            {
                //set graph model of internal brain to selected text asset
                CoreBrainInternal internalBrain = (CoreBrainInternal)brain.coreBrain;
                if (internalBrain != null)
                {
                    internalBrain.graphModel = selectedTextAsset;
                }
                else
                {
                    Debug.LogError("Internal brain not found!");
                }
            }
            else
            {
                Debug.LogWarning("TextAsset for internal brain's graph model isn't selected!");
            }
        }
        else
        {
            Debug.LogError("Brain not found on current scene!");
        }
    }

    private static Scene GetTrainingScene()
    {
        Scene trainingScene;

        //if scene name isn't null, try to get it
        if (!string.IsNullOrEmpty(SceneName))
        {
            trainingScene = SceneManager.GetSceneByName(SceneName);
            if (!trainingScene.IsValid())//if scene isn't found get active scene
            {
                Debug.LogWarning("Scene with name " + SceneName + " not found, using active scene for build.");
                trainingScene = SceneManager.GetActiveScene();
            }
        }
        else
        {
            Debug.LogWarning("SceneName parameter not filled, using active scene for build.");
            trainingScene = SceneManager.GetActiveScene();
        }

        return trainingScene;
    }
}
