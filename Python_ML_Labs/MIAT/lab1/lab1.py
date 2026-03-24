import random
import re
import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.model_selection import train_test_split

DOMAIN_KEYWORDS = {
    "economy": ["економіка", "інвестиції", "податки", "бізнес", "бюджет", "інфляція"],
    "social": ["освіта", "медицина", "пенсії", "соціальний", "захист"],
    "government": ["реформи", "децентралізація", "корупція", "судова", "влада"],
    "defense": ["армія", "безпека", "оборона", "нато"],
    "international": ["єс", "інтеграція", "дипломатія"]
}

ALL_KEYWORDS = list(set(sum(DOMAIN_KEYWORDS.values(), [])))

FILLER_PHRASES = [
    "ми забезпечимо розвиток",
    "будемо сприяти покращенню",
    "гарантуємо стабільність",
    "підвищимо рівень життя",
    "реалізуємо ефективні рішення"
]

def generate_text():
    keywords = random.sample(ALL_KEYWORDS, k=random.randint(4, 6))
    filler = random.sample(FILLER_PHRASES, k=2)

    text = " ".join(filler + keywords)

    return text, keywords


def generate_dataset(size):
    texts = []
    labels = []

    for _ in range(size):
        t, k = generate_text()
        texts.append(t)
        labels.append(k)

    return pd.DataFrame({
        "text": texts,
        "keywords": labels
    })


STOPWORDS = set([
    "ми", "будемо", "забезпечимо", "покращимо", "гарантуємо",
    "та", "і", "в", "на", "для", "це", "з"
])


def preprocess(text):
    text = text.lower()
    text = re.sub(r"[^\w\s]", "", text)

    tokens = text.split()
    tokens = [t for t in tokens if t not in STOPWORDS]

    return " ".join(tokens)


class KeywordExtractor:
    def __init__(self):
        self.vectorizer = TfidfVectorizer(
            ngram_range=(1, 2),
            min_df=1,
            max_df=0.9
        )

    def fit(self, texts):
        processed = [preprocess(t) for t in texts]
        self.vectorizer.fit(processed)

    def extract(self, texts, top_k=5):
        processed = [preprocess(t) for t in texts]
        tfidf_matrix = self.vectorizer.transform(processed)
        feature_names = self.vectorizer.get_feature_names_out()

        results = []

        for row in tfidf_matrix:
            scores = zip(feature_names, row.toarray()[0])
            sorted_words = sorted(scores, key=lambda x: x[1], reverse=True)

            top_words = [w for w, s in sorted_words[:top_k] if s > 0]
            results.append(top_words)

        return results


def evaluate(predicted, actual):
    precisions = []
    recalls = []
    f1s = []

    for pred, act in zip(predicted, actual):
        pred_set = set(pred)
        act_set = set(act)

        tp = len(pred_set & act_set)
        fp = len(pred_set - act_set)
        fn = len(act_set - pred_set)

        precision = tp / (tp + fp) if (tp + fp) > 0 else 0
        recall = tp / (tp + fn) if (tp + fn) > 0 else 0

        if precision + recall == 0:
            f1 = 0
        else:
            f1 = 2 * precision * recall / (precision + recall)

        precisions.append(precision)
        recalls.append(recall)
        f1s.append(f1)

    return {
        "precision": sum(precisions) / len(precisions),
        "recall": sum(recalls) / len(recalls),
        "f1": sum(f1s) / len(f1s)
    }


def main():
    df = generate_dataset(100)

    train_df, test_df = train_test_split(df, test_size=0.3, random_state=42)

    train_df.to_csv("train.csv", index=False)
    test_df.to_csv("test.csv", index=False)

    extractor = KeywordExtractor()
    extractor.fit(train_df["text"])

    predicted = extractor.extract(test_df["text"].tolist())

    metrics = evaluate(predicted, test_df["keywords"].tolist())

    print("=== RESULTS ===")
    print(f"Precision: {metrics['precision']:.2f}")
    print(f"Recall: {metrics['recall']:.2f}")
    print(f"F1-score: {metrics['f1']:.2f}")

    print("\n=== SAMPLE ===")
    for i in range(5):
        print("\nTEXT:", test_df.iloc[i]["text"])
        print("EXPECTED:", test_df.iloc[i]["keywords"])
        print("PREDICTED:", predicted[i])


if __name__ == "__main__":
    main()