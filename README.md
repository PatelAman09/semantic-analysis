# **Semantic Analysis Of Textual Data**

## **Project Overview**
The Semantic Analysis Project is a pipeline designed to process documents, extract meaningful data, and perform semantic analysis. The project involves cleaning and filtering text, generating embeddings, calculating cosine similarity, and visualizing the results through charts and heatmaps.

---

## **Project Workflow**

### 1. **Document Cleaning and Filtering**
   - **Input**: Raw document (e.g., text, PDF, or any unstructured data).
   - **Process**:
     - Text cleaning: Remove noise, special characters, and unnecessary formatting.
     - Filtering: Extract relevant information for analysis.
   - **Output**: A structured JSON file representing the cleaned and filtered data.

---

### 2. **Embedding Generation**
   - **Input**: JSON file created in the previous step.
   - **Process**:
     - Upload the JSON file.
     - Generate embeddings for the text using OpenAI's `text-embedding-ada-002` model.
   - **Output**: A CSV file with two columns:
     - **Text**: The original extracted data.
     - **Embedding**: Numerical vectors representing semantic meaning.

---

### 3. **Cosine Similarity Calculation**
   - **Input**: CSV file with embeddings.
   - **Process**:
     - Calculate the cosine similarity between embeddings to measure the semantic similarity between text data points.
   - **Output**: A CSV file representing a similarity matrix.

---

### 4. **Visualization**
   - **Input**: Cosine similarity CSV file.
   - **Process**:
     - Generate visualizations (e.g., heatmaps, bar charts) for analyzing and presenting results.
     - Tools: Excel, Python (Matplotlib, Seaborn), or any preferred data visualization library.
   - **Output**: Graphical representations of semantic similarities.

---

## **Features**
- Automated processing of documents into structured JSON data.
- Embedding generation using OpenAI’s cutting-edge `text-embedding-ada-002` model.
- Semantic similarity analysis through cosine similarity calculations.
- Easy-to-interpret visualizations like bar charts and heatmaps.

---

## **Setup Instructions**

### 1. **Prerequisites**
- **Programming Language**: C#
- **Dependencies**:
  - Newtonsoft.Json (for JSON handling)
  - HttpClient (for API calls)
  - Tools for visualization (e.g., Python libraries or Excel)

### 2. **Installation**
- Clone the repository:
  ```bash
  git clone <repository_url>
  cd <project_folder>
  ```
- Install dependencies:
  ```bash
  dotnet add package Newtonsoft.Json
  ```

---

## **Usage Instructions**

### Step 1: Document Cleaning and JSON Creation
1. Place your raw document in the designated folder (`/input`).
2. Run the cleaning and filtering script to generate a structured JSON file.

### Step 2: Upload JSON for Embeddings
1. Specify the path to your JSON file in the configuration or runtime input.
2. Run the embedding generation script to create the embeddings CSV.

### Step 3: Calculate Cosine Similarity
1. Use the embeddings CSV as input for the cosine similarity function.
2. Generate the similarity matrix CSV.

### Step 4: Visualization
1. Load the similarity matrix CSV into the visualization tool.
2. Generate and analyze the heatmap or bar charts.

---

## **Examples**

### **Sample Workflow**
1. Input:  
   Raw document with text like:
   ```
   Climate change is a pressing global issue.
   Renewable energy is key to a sustainable future.
   ```

2. Output:
   - JSON file:
     ```json
     [
       { "text": "Climate change is a pressing global issue." },
       { "text": "Renewable energy is key to a sustainable future." }
     ]
     ```
   - Embeddings CSV:
     ```
     Text,Embedding
     "Climate change is a pressing global issue.","[0.123, 0.456, 0.789]"
     "Renewable energy is key to a sustainable future.","[0.234, 0.567, 0.890]"
     ```
   - Cosine similarity CSV:
     ```
     ,0,1
     0,1.0,0.89
     1,0.89,1.0
     ```
   - Heatmap:  
     A graphical representation of similarity scores.


