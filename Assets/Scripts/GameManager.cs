using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

    [SerializeField] Image[] digitDisplayImage = new Image[kCodeDigitCount];
    [SerializeField] TMP_Text hintsText = null;
    [SerializeField] RectTransform hintsTextPanel = null;
    [SerializeField] TMP_Text livesText = null;
    [SerializeField] Scrollbar scrollbar = null;
    [SerializeField] UnityEvent onGameOverEvent = null;
    [SerializeField] Color selectedDigitColor = Color.gray;

    readonly int[] allTheDigits = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    const int kCodeDigitCount = 4;
    const int kMaxTryCount = 5;
    const int kMaxTextLineCount = 10;

    int[] secretCode = new int[kCodeDigitCount];
    int secretCodeNumber;
    Difficulty difficulty;
    List<int> userCode = new List<int>(kCodeDigitCount);
    int selectedDigitIndex;
    int lives = kMaxTryCount;
    int hintsTextLineCount;
    float hintsTextPanelHeight;
    float startTextOffsetY;
    RectTransform hintsTextRectTransform;
    TMP_Text[] digitTexts = new TMP_Text[kCodeDigitCount];

    public enum Difficulty {
        Easy,
        Hard
    }

    void Start() {
        GenerateSecretCode();
        livesText.text = "Essais restant: " + lives;
        hintsTextPanelHeight = hintsTextPanel.rect.height;
        hintsTextRectTransform = hintsText.rectTransform;
        startTextOffsetY = hintsTextRectTransform.offsetMax.y;
        for (int i = 0; i < kCodeDigitCount; i++) {
            digitTexts[i] = digitDisplayImage[i].GetComponentInChildren<TMP_Text>();
        }
        Debug.Log(secretCodeNumber);
    }

    void GenerateSecretCode() {
        List<int> possibleDigits = new List<int>(allTheDigits);
        int digitRank = 1;
        for (int i = kCodeDigitCount - 1; i >= 0; i--) {
            int randomIndex = Random.Range(0, possibleDigits.Count);
            secretCode[i] = possibleDigits[randomIndex];
            secretCodeNumber += possibleDigits[randomIndex] * digitRank;
            possibleDigits.RemoveAt(randomIndex);
            digitRank *= 10;
        }
    }

    public void SetDifficulty(int difficulty) {
        this.difficulty = (Difficulty)difficulty;
    }

    public void SetUserCodeDigit(int inputDigit) {
        if (userCode.Count < kCodeDigitCount) {
            userCode.Add(inputDigit);
        } else {
            userCode[selectedDigitIndex] = inputDigit;
        }
        digitTexts[selectedDigitIndex].text = inputDigit.ToString();
        SetSelectedDigitIndex((selectedDigitIndex + 1) % kCodeDigitCount);
    }

    public void SetSelectedDigitIndex(int index) {
        selectedDigitIndex = index;
        for (int i = 0; i < kCodeDigitCount; i++) {
            digitDisplayImage[i].color = Color.white;
        }
        digitDisplayImage[index].color = selectedDigitColor;
    }

    public void ValidateUserCode() {
        if (userCode.Count < kCodeDigitCount) {
            return;
        }
        int bullsCount = 0, cowsCount = 0;
        for (int i = 0; i < kCodeDigitCount; i++) {
            if (secretCode.Contains(userCode[i])) {
                if (secretCode[i] == userCode[i]) {
                    bullsCount++;
                } else {
                    cowsCount++;
                    if (difficulty == Difficulty.Easy) {
                        hintsText.text += userCode[i] + " est dans le code !\n";
                        hintsTextLineCount++;
                    }
                }
            }
        }
        hintsText.text += "Il y a " + cowsCount + " vache(s) et " + bullsCount + " taureau(x).\n";
        hintsTextLineCount++;
        if (bullsCount == kCodeDigitCount) {
            hintsText.text += "<color=yellow>Vous avez gagné !!</color>";
            onGameOverEvent.Invoke();
        } else {
            lives--;
            if (lives > 0) {
                livesText.text = "Essais restant: " + lives;
                if (lives == 2) {
                    hintsText.text += "<color=green>Aide:</color> Le chiffre " + secretCode[0] +
                        " est en première position.\n";
                    hintsTextLineCount++;
                } else if (lives == 1) {
                    hintsText.text += "<color=green>Aide:</color> Le chiffre " + secretCode[3] +
                        " est en dernière position.\n";
                    hintsTextLineCount++;
                }
            } else {
                hintsText.text += "<color=red>Vous avez perdu !</color> Le code était: <color=orange>" +
                    secretCodeNumber + "</color>";
                onGameOverEvent.Invoke();
            }
        }
        scrollbar.size = Mathf.Min(1f, (float)kMaxTextLineCount / hintsTextLineCount);
        if (scrollbar.size < 1f) {
            UpdateScrolling(1f);
            scrollbar.value = 1f;
        }
    }

    public void UpdateScrolling(float t) {
        float overflowTextHeight = hintsText.preferredHeight - hintsTextPanelHeight;
        float newOffsetY = Mathf.Lerp(startTextOffsetY, overflowTextHeight - startTextOffsetY, t);
        hintsTextRectTransform.offsetMax = new Vector2(0f, newOffsetY);
    }

    public void Restart() {
        userCode.Clear();
        for (int i = 0; i < kCodeDigitCount; i++) {
            digitTexts[i].text = string.Empty;
        }
        lives = kMaxTryCount;
        livesText.text = lives.ToString();
        hintsTextLineCount = 0;
        hintsText.text = string.Empty;
        scrollbar.size = 1f;
        hintsTextRectTransform.offsetMax = new Vector2(0f, startTextOffsetY);
    }
}
