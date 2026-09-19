using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ButchersGames;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>Runs real CharacterController/trigger checks in a disposable Play Mode session.</summary>
[InitializeOnLoad]
public static class Stage5PlayChecks
{
    const string Key = "Stage5Checks.Active";
    static IEnumerator checks;
    static double deadline;
    static string error;
    static GameManager game;
    static LevelManager levels;
    static PlayerMovement player;
    static PlayerWealth wealth;

    static Stage5PlayChecks()
    {
        EditorApplication.playModeStateChanged += Changed;
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Gameplay.unity");
        SessionState.SetBool(Key, true);
        SessionState.SetBool(Key + ".HadSave", PlayerPrefs.HasKey("RunRich.LevelIndex"));
        SessionState.SetInt(Key + ".Save", PlayerPrefs.GetInt("RunRich.LevelIndex", 0));
        SessionState.SetInt(Key + ".Exit", 1);
        EditorApplication.EnterPlaymode();
    }

    static void Changed(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Key, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            deadline = EditorApplication.timeSinceStartup + 150;
            checks = CheckAll();
            Application.logMessageReceived += Log;
            EditorApplication.update += Tick;
        }
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (SessionState.GetBool(Key + ".HadSave", false)) PlayerPrefs.SetInt("RunRich.LevelIndex", SessionState.GetInt(Key + ".Save",0));
            else PlayerPrefs.DeleteKey("RunRich.LevelIndex");
            PlayerPrefs.Save();
            SessionState.SetBool(Key, false);
            EditorApplication.Exit(SessionState.GetInt(Key + ".Exit", 1));
        }
    }

    static void Log(string message, string stack, LogType type)
    {
        if(type == LogType.Exception || type == LogType.Error || type == LogType.Assert) error = message + "\n" + stack;
    }
    static void Tick()
    {
        try
        {
            if (error != null) throw new Exception(error);
            if(EditorApplication.timeSinceStartup > deadline) throw new Exception("Play checks timeout");
            if(checks.MoveNext()) return;
            File.AppendAllText("Documentation/Stage5/PlayChecks.txt", "ALL PLAY CHECKS PASSED\n");
            SessionState.SetInt(Key + ".Exit",0);
        }
        catch(Exception e)
        {
            File.AppendAllText("Documentation/Stage5/PlayChecks.txt", "FAILED: " + e + "\n");
            Debug.LogException(e);
        }
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= Log;
        EditorApplication.ExitPlaymode();
    }
    static void Require(bool condition,string message)
    {
        if(!condition) throw new Exception(message);
        File.AppendAllText("Documentation/Stage5/PlayChecks.txt","PASS " + message + "\n");
    }
    static void Warp(Vector3 position)
    {
        var controller=player.GetComponent<CharacterController>();
        controller.enabled=false; player.transform.position=position; controller.enabled=true;
        Physics.SyncTransforms();
    }
    static IEnumerator CheckAll()
    {
        File.WriteAllText("Documentation/Stage5/PlayChecks.txt", "Stage 5 physics/play-mode validation\n");
        yield return null;
        game=Object.FindFirstObjectByType<GameManager>();
        levels=Object.FindFirstObjectByType<LevelManager>();
        player=Object.FindFirstObjectByType<PlayerMovement>();
        wealth=player.GetComponent<PlayerWealth>();
        levels.SelectLevel(0);
        yield return null;
        Require(game.State==GameState.WaitingToStart && !player.IsRunning,"Loaded level waits for Start");
        game.StartGame();
        Warp(new Vector3(0,0,22.7f));
        var checkpoint=levels.CurrentLevel.GetComponentInChildren<FlagCheckpoint>();
        var flag=(Transform[])typeof(FlagCheckpoint).GetField("flags",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(checkpoint);
        float initial=flag[0].localPosition.y;
        float until=Time.time+.35f;
        while(Time.time<until) yield return null;
        player.SetRunning(false);
        until=Time.time+.8f;
        while(Time.time<until) yield return null;
        Require(flag[0].localPosition.y>initial+1.9f,"Flag raised by real controller trigger");
        Require(wealth.Value==10,"Flag does not change wealth");
        game.Win(); game.RestartLevel(); yield return null;
        checkpoint=levels.CurrentLevel.GetComponentInChildren<FlagCheckpoint>();
        flag=(Transform[])typeof(FlagCheckpoint).GetField("flags",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(checkpoint);
        Require(Mathf.Abs(flag[0].localPosition.y-initial)<.01f,"Flags reset after restart");

        game.StartGame(); Warp(new Vector3(0,0,2.8f));
        until=Time.time+.5f; while(Time.time<until) yield return null;
        player.SetRunning(false);
        Require(wealth.Value==20,"Money pickup changes wealth by +10");
        game.Win(); game.RestartLevel(); yield return null;
        game.StartGame(); wealth.Add(40); Warp(new Vector3(-1.55f,0,10.6f));
        until=Time.time+.5f; while(Time.time<until) yield return null;
        player.SetRunning(false);
        Require(wealth.Value==40,"Obstacle applies -10 once");
        game.Win(); game.RestartLevel(); yield return null;

        foreach(int amount in new[]{10,20,40,60,80})
        {
            if(game.State==GameState.Playing) game.Win();
            if(game.State==GameState.Won || game.State==GameState.Lost) game.RestartLevel();
            yield return null;
            game.StartGame(); wealth.Add(amount-wealth.Value);
            var gates=levels.CurrentLevel.GetComponentsInChildren<FinishGate>().OrderBy(g=>g.transform.position.z).ToArray();
            for(int i=0;i<gates.Length && game.State==GameState.Playing;i++)
            {
                Warp(new Vector3(0,0,gates[i].transform.position.z-5));
                until=Time.time+1.5f;
                while(Time.time<until && game.State==GameState.Playing) yield return null;
            }
            if(game.State==GameState.Playing)
            {
                Warp(new Vector3(0,0,94.8f));
                until=Time.time+.7f; while(Time.time<until && game.State==GameState.Playing) yield return null;
            }
            int expected=amount/20+1;
            Require(game.State==GameState.Won && game.FinishMultiplier==expected,"Wealth " + amount + " finishes with x" + expected);
        }
        game.NextLevel(); yield return null;
        Require(levels.CurrentLevelIndex==1 && game.State==GameState.WaitingToStart && game.FinishMultiplier==1,"Next level loads and resets multiplier");
        Require(levels.CurrentLevel.GetComponentsInChildren<FinishGate>().Length==4,"Second level has all gate stages");
        game.StartGame(); wealth.Add(-100);
        Require(game.State==GameState.Lost && !player.IsRunning,"Zero wealth triggers defeat");
        game.RestartLevel(); yield return null;
        Require(wealth.Value==10 && game.State==GameState.WaitingToStart,"Defeat restart restores wealth");
    }
}
