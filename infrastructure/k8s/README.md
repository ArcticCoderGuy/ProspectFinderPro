# Kubernetes Deployment

This directory will contain Kubernetes manifests for production deployment.

## Planned Structure
- `namespace.yaml` - Namespace definition
- `configmap.yaml` - Configuration maps
- `secrets.yaml` - Secrets management
- `deployment-api.yaml` - API Gateway deployment
- `deployment-webapp.yaml` - Web App deployment  
- `service-api.yaml` - API Gateway service
- `service-webapp.yaml` - Web App service
- `ingress.yaml` - Ingress configuration
- `hpa.yaml` - Horizontal Pod Autoscaler

## Usage
```bash
# Apply all manifests
kubectl apply -f infrastructure/k8s/

# Check status
kubectl get pods -n prospectfinderpro
```