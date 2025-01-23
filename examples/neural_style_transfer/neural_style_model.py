import torch
import torch.nn as nn
import torch.optim as optim
import torchvision.transforms as transforms
import torchvision.models as models
from PIL import Image

device = "cuda" if torch.cuda.is_available() else "cpu"
print(f"Using device: {device}")

def load_image(image_path, size=None):
    image = Image.open(image_path).convert('RGB')

    if size is not None:
        image = image.resize((size,size))
    transform = transforms.Compose([
        transforms.ToTensor()
    ])

    return transform(image).unsqueeze(0).to(device)


class ContentLoss(nn.Module):
    def __init__(self, target):
        super(ContentLoss, self).__init__()
        self.target = target.detach()

    def forward(self, input):
        self.loss = nn.functional.mse_loss(input, self.target)
        return input

def gram_matrix(input):
    batch_size, channels, height, width = input.size()
    features = input.view(batch_size * channels, height * width)
    gram = torch.mm(features, features.t())
    return gram.div(batch_size * channels * height * width)

class StyleLoss(nn.Module):
    def __init__(self, target_feature):
        super(StyleLoss, self).__init__()
        self.target = gram_matrix(target_feature).detach()

    def forward(self, input):
        G = gram_matrix(input)
        self.loss = nn.functional.mse_loss(G, self.target)
        return input

def get_style_model_and_losses(style_img, content_img):
    cnn = models.vgg19(pretrained=True).features.to(device).eval()

    normalization_mean = torch.tensor([0.485, 0.456, 0.406]).to(device)
    normalization_std = torch.tensor([0.229, 0.224, 0.225]).to(device)
    normalization = transforms.Normalize(normalization_mean, normalization_std)

    content_layers = ['conv_4']
    style_layers = ['conv_1', 'conv_2', 'conv_3', 'conv_4', 'conv_5']

    content_losses = []
    style_losses = []

    model = nn.Sequential(normalization)

    i = 0

    for layer in cnn.children():
        if isinstance(layer, nn.Conv2d):
            i += 1
            name = f'conv_{i}'
        elif isinstance(layer, nn.ReLU):
            name = f'relu_{i}'
            layer = nn.ReLU(inplace=False)

        elif isinstance(layer, nn.MaxPool2d):
            name = f'pool_{i}'
        elif isinstance(layer, nn.BatchNorm2d):
            name = f'bn_{i}'

        else:
            raise RuntimeError(f"Not a valid layer: {layer.__class_.__name__}")

        model.add_module(name, layer)

        if name in content_layers:
            target = model(content_img).detach()
            content_loss = ContentLoss(target)
            model.add_module(f'content_loss_{i}', content_loss)
            content_losses.append(content_loss)

        if name in style_layers:
            target_feature = model(style_img).detach()
            style_loss = StyleLoss(target_feature)
            model.add_module(f'style_loss_{i}', style_loss)
            style_losses.append(style_loss)


    for i in range(len(model)-1, -1, -1):
        if isinstance(model[i], ContentLoss) or isinstance(model[i], StyleLoss):
            break

    model = model[:(i + 1)]
    return model, style_losses, content_losses

def run_style_transfer(content_img, style_img, num_steps=300, style_weight=1e6, content_weight=1):
    model, style_losses, content_losses = get_style_model_and_losses(style_img, content_img)

    input_img = content_img.clone()
    optimizer = optim.LBFGS([input_img.requires_grad_()])

    run = [0]

    while run[0] <= num_steps:
        def closure():
            with torch.no_grad():
                input_img.clamp_(0,1)

            optimizer.zero_grad()
            model(input_img)

            style_score = 0
            content_score = 0

            for sl in style_losses:
                style_score += sl.loss
            for cl in content_losses:
                content_score += cl.loss


            style_score *= style_weight
            content_score *= content_weight

            loss = style_score + content_score

            loss.backward()

            run[0] += 1

            if run[0] % 50 == 0:
                print(f"Run {run[0]}:")
                print(f"Style loss: {style_score.item():.4f} Content Loss: {content_score.item():4f}")
                print("-----------------")


            return loss
        optimizer.step(closure)
    with torch.no_grad():
        input_img.clamp_(0,1)

    return input_img

def neural_style_transfer(content_path, style_path, output_path, num_steps=300):
    content_img = load_image(content_path)
    style_img = load_image(style_path, size=content_img.shape[2])

    output = run_style_transfer(content_img, style_img, num_steps=num_steps)

    output_img = output.cpu().squeeze(0)
    output_img = transforms.ToPILImage()(output_img)
    output_img.save(output_path)
    print("Image saved")


if __name__ == "__main__":
    content_path = "field01.jpg"
    style_path = "van01.jpg"
    output_path = "./style_output.jpg"
    neural_style_transfer(content_path, style_path, output_path)










