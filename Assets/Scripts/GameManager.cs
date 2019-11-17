using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    [SerializeField] TMP_Text[] digitTexts = new TMP_Text[kCodeDigitCount];
    [SerializeField] TMP_Text hintsText = null;
    [SerializeField] RectTransform hintsTextPanel = null;
    [SerializeField] TMP_Text livesText = null;
    [SerializeField] Scrollbar scrollbar = null;

    readonly int[] allTheDigits = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    const int kCodeDigitCount = 4;
    const int kMaxTryCount = 5;
    const int kMaxTextLineCount = 10;

    int[] secretCode = new int[kCodeDigitCount];
    Difficulty difficulty;
    List<int> userCode = new List<int>(kCodeDigitCount);
    int selectedDigitIndex;
    int lives = kMaxTryCount;
    int hintsTextLineCount;
    float hintsTextPanelHeight;
    float startTextOffsetY;

    public enum Difficulty {
        Easy,
        Hard
    }

    void Start() {
        GenerateSecretCode();
        livesText.text = "Essais restant: " + lives;
        hintsTextPanelHeight = hintsTextPanel.rect.height;
        startTextOffsetY = hintsText.rectTransform.offsetMax.y;
        Debug.Log(secretCode[0].ToString() + secretCode[1] + secretCode[2] + secretCode[3]);
    }

    void GenerateSecretCode() {
        List<int> possibleDigits = new List<int>(allTheDigits);
        for (int i = 0; i < kCodeDigitCount; i++) {
            int randomIndex = Random.Range(0, possibleDigits.Count);
            secretCode[i] = possibleDigits[randomIndex];
            possibleDigits.RemoveAt(randomIndex);
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
        selectedDigitIndex = (selectedDigitIndex + 1) % kCodeDigitCount;
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
            Debug.Log("Vous avez gagné !!");
        } else {
            lives--;
            if (lives > 0) {
                livesText.text = "Essais restant: " + lives;
                if (lives == 2) {
                    hintsText.text += "Aide: Le chiffre " + secretCode[0] + " est en première position.\n";
                    hintsTextLineCount++;
                } else if (lives == 1) {
                    hintsText.text += "Aide: Le chiffre " + secretCode[3] + " est en dernière position.\n";
                    hintsTextLineCount++;
                }
            } else {
                Debug.Log("Vous avez perdu ! Le code était: ");
            }
        }
        scrollbar.size = Mathf.Min(1f, (float)kMaxTextLineCount / hintsTextLineCount);
    }

    public void OnScrollbarChange(float t) {
        float overflowTextHeight = hintsText.preferredHeight - hintsTextPanelHeight;

        RectTransform rectTransform = hintsText.rectTransform;
        float newOffsetY = Mathf.Lerp(startTextOffsetY, overflowTextHeight - startTextOffsetY, t);
        rectTransform.offsetMax = new Vector2(0f, newOffsetY);
    }

    void Restart() {
        userCode.Clear();
        for (int i = 0; i < kCodeDigitCount; i++) {
            digitTexts[i].text = string.Empty;
        }
        lives = kMaxTryCount;
        hintsTextLineCount = 0;
    }
}
