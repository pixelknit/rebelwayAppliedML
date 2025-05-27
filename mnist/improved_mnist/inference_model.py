import torch
import torch.nn as nn
from torch.utils.data import DataLoader

from torchvision import datasets, transforms
# from torchvision.transforms import ToTensor, Lambda, Compose
import matplotlib.pyplot as plt
from mnist import Net
from PIL import Image


device = torch.device("mps")

model = Net().to(device)
print(model)


model = Net().to(device)
model.load_state_dict(torch.load("mnist_base_model.pth"))
model.eval()

image_path = f"7.png"
image = Image.open(image_path).convert("L")

transform  = transforms.Compose([
    transforms.ToTensor(),
    transforms.Resize((28, 28)),

])

image = transform(image).unsqueeze(0).to(device)

with torch.no_grad():
  output = model(image)
  prediction = output.argmax(1).item()

print(f"Prediction: {prediction}")

