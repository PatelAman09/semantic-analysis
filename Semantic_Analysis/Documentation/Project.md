
# **Semantic Analysis Of Textual Data**

## **Project Overview**

The Semantic Analysis Project is a pipeline designed to process documents, extract meaningful data, and perform semantic analysis. The project involves cleaning and filtering text, generating embeddings, calculating cosine similarity, and visualizing the results through scatter plot.

---

## **Project Workflow**

### 1. **Document Cleaning and Extraction**

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
  - Generate visualizations scatter plots for analyzing and presenting results.
- **Output**: Graphical representations of semantic similarities.

---

## **Project Structure**

```
📂 Semantic_Analysis
│-- 📂 CSVOutput/              # Folder storing CSV files
│-- 📂 EmbeddingOutput/        # Folder storing generated embeddings
│-- 📂 ExtractedData/          # Folder containing input documents
│-- 📂 ScatterPlot/            # Folder storing visualization results
│-- 📂 Interfaces/             # Folder containing interface definitions
│-- 📂 RawData/                # Folder for unprocessed files
│-- 📜 appsettings.json        # Configuration file
│-- 📜 CosineSimilarity.cs     # Script for cosine similarity calculation
│-- 📜 DataExtraction.cs       # Script for document processing
│-- 📜 EmbeddingProcessor.cs   # Script for generating embeddings
│-- 📜 Program.cs              # Main entry point
│-- 📜 Visualization.cs        # Script for visualization

📂 UnitTestProject
│-- 📜 CosineSimilarityUnitTest.cs  # Unit tests for cosine similarity
│-- 📜 DataExtractionTest.cs        # Unit tests for data extraction
│-- 📜 EmbeddingUnitTest.cs         # Unit tests for embedding processor
```

---

## **Dependencies**

### **For Data Extraction:**

```csharp
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

```

### **For Embedding Processor:**

```csharp
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Embeddings;
using Semantic_Analysis.Interfaces;
```

### **For Cosine Similarity:**

```csharp
using Microsoft.Extensions.Configuration;
using Semantic_Analysis.Interfaces;
using System.Globalization;
using System.Text;
```

---

## **Setup Instructions**

### 1. **Prerequisites**

- **Programming Language**: .NET Core
- **Dependencies**: Install required NuGet packages

---

## **Usage Instructions**

1. Place your raw documents in the `RawData/` folder.  
2. Open a CMD or any Terminal and navigate to the project root.  
3. Build the project using:  
   ```sh
   dotnet build
4. Navigate to the output directory:
   ```sh
   cd bin/Debug/{YourDotNetVersion}/
5. Run the executable:
   ```sh
   ./Semantic_Analysis.exe
---

### Sample Output Files  

To provide reference outputs, the project includes a `sample_outputs/` folder.  
This folder contains:  
- **Raw Data**: Input documents.  
- **Cosine Similarity**: Precomputed similarity matrices for testing.  
- **Visualizations**: Scatter plot charts for understanding the analysis results.
---

## **Team Members**

-  Muhammad Ahsan Ijaz
-  Aman Basha Patel
-  Saquib Attar

---
